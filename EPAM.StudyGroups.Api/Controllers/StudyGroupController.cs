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

        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateStudyGroup(CreateStudyGroupRequest studyGroup)
        {
            var groups = await _studyGroupRepository
                .GetStudyGroups()
                .ConfigureAwait(false);

            if (groups.FirstOrDefault(g => g.Name == studyGroup.Name) != null)
            {
                return new ConflictResult();
            }

            // AC: 1
            if (groups.FirstOrDefault(g => g.Subject == studyGroup.Subject) != null)
            {
                return new ConflictResult();
            }

            await _studyGroupRepository
                .CreateStudyGroup(
                    new StudyGroup
                    {
                        Name = studyGroup.Name,
                        Subject = studyGroup.Subject,
                        CreateDate = DateTime.UtcNow,
                    })
                .ConfigureAwait(false);

            return new OkResult();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStudyGroups()
        {
            var studyGroups = await _studyGroupRepository
                .GetStudyGroups()
                .ConfigureAwait(false);

            return new OkObjectResult(studyGroups);
        }

        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchStudyGroups(string subject)
        {
            var studyGroups = await _studyGroupRepository
                .SearchStudyGroups(subject)
                .ConfigureAwait(false);

            return new OkObjectResult(studyGroups);
        }

        [HttpPut("join/{studyGroupId}/{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> JoinStudyGroup(int studyGroupId, int userId)
        {
            if (null == (await _studyGroupRepository
                    .GetStudyGroups()
                    .ConfigureAwait(false))
                    .FirstOrDefault(g => g.StudyGroupId == studyGroupId))
            {
                return new NotFoundResult();
            }

            if (null == (await _userRepository
                    .GetUsers()
                    .ConfigureAwait(false))
                    .FirstOrDefault(u => u.Id == userId))
            {
                return new NotFoundResult();
            }

            await _studyGroupRepository
                .JoinStudyGroup(studyGroupId, userId)
                .ConfigureAwait(false);

            return new OkResult();
        }

        [HttpPost("leave")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LeaveStudyGroup(int studyGroupId, int userId)
        {
            await _studyGroupRepository
                .LeaveStudyGroup(studyGroupId, userId)
                .ConfigureAwait(false);

            return new OkResult();
        }
    }
}