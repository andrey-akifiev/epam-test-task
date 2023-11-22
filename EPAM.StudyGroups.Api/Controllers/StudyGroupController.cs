using EPAM.StudyGroups.Api.Models;
using EPAM.StudyGroups.Data.DAL;
using EPAM.StudyGroups.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace EPAM.StudyGroups.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StudyGroupController
    {
        private readonly IStudyGroupRepository _studyGroupRepository;
        private readonly IUserRepository _userRepository;

        public StudyGroupController(
            IStudyGroupRepository studyGroupRepository,
            IUserRepository userRepository)
        {
            _studyGroupRepository = studyGroupRepository ?? throw new ArgumentNullException(nameof(studyGroupRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateStudyGroup(CreateStudyGroupRequest studyGroup, CancellationToken ctn)
        {
            var newGroup = new StudyGroup
            {
                Name = studyGroup.Name,
                // Part of implementation of AC1b:
                // The only valid Subjects are: Math, Chemistry, Physics
                Subject = (Subject)Enum.Parse(typeof(Subject), studyGroup.Subject),
                // AC1c: We want to record when Study Groups were created
                CreateDate = DateTime.UtcNow,
            };

            IEnumerable<StudyGroup> groups = await _studyGroupRepository
                .GetStudyGroups(ctn)
                .ConfigureAwait(false);

            if (groups.FirstOrDefault(g => g.Name == newGroup.Name) != null)
            {
                return new ConflictResult();
            }

            // AC1: Users are able to create only one Study Group for a single Subject
            if (groups.FirstOrDefault(g => g.Subject == newGroup.Subject) != null)
            {
                return new ConflictResult();
            }

            await _studyGroupRepository
                .CreateStudyGroup(newGroup, ctn)
                .ConfigureAwait(false);

            return new OkResult();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStudyGroups(CancellationToken ctn)
        {
            var studyGroups = await _studyGroupRepository
                .GetStudyGroups(ctn)
                .ConfigureAwait(false);

            return new OkObjectResult(studyGroups);
        }

        [HttpGet("{subject}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchStudyGroups(string subject, CancellationToken ctn)
        {
            var studyGroups = await _studyGroupRepository
                .SearchStudyGroups(subject, ctn)
                .ConfigureAwait(false);

            return new OkObjectResult(studyGroups);
        }

        [HttpPut("join/{studyGroupId}/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> JoinStudyGroup(int studyGroupId, int userId, CancellationToken ctn)
        {
            if (null == (await _studyGroupRepository
                    .GetStudyGroups(ctn)
                    .ConfigureAwait(false))
                    .FirstOrDefault(g => g.StudyGroupId == studyGroupId))
            {
                return new NotFoundResult();
            }

            if (null == (await _userRepository
                    .GetUsers(ctn)
                    .ConfigureAwait(false))
                    .FirstOrDefault(u => u.Id == userId))
            {
                return new NotFoundResult();
            }

            await _studyGroupRepository
                .JoinStudyGroup(studyGroupId, userId, ctn)
                .ConfigureAwait(false);

            return new OkResult();
        }

        [HttpPost("leave")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LeaveStudyGroup(int studyGroupId, int userId, CancellationToken ctn)
        {
            await _studyGroupRepository
                .LeaveStudyGroup(studyGroupId, userId, ctn)
                .ConfigureAwait(false);

            return new OkResult();
        }
    }
}