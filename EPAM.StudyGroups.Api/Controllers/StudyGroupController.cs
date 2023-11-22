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

        public StudyGroupController(IStudyGroupRepository studyGroupRepository)
        {
            _studyGroupRepository = studyGroupRepository;
        }

        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateStudyGroup(CreateStudyGroupRequest studyGroup)
        {
            if ((await _studyGroupRepository
                .GetStudyGroups()
                .ConfigureAwait(false))
                .FirstOrDefault(g => g.Name == studyGroup.Name) != null)
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
        public async Task<IActionResult> GetStudyGroups()
        {
            var studyGroups = await _studyGroupRepository
                .GetStudyGroups()
                .ConfigureAwait(false);
            return new OkObjectResult(studyGroups);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchStudyGroups(string subject)
        {
            var studyGroups = await _studyGroupRepository
                .SearchStudyGroups(subject)
                .ConfigureAwait(false);
            return new OkObjectResult(studyGroups);
        }

        [HttpPut("join/{studyGroupId}/{userId}")]
        public async Task<IActionResult> JoinStudyGroup(int studyGroupId, int userId)
        {
            await _studyGroupRepository.JoinStudyGroup(studyGroupId, userId);
            return new OkResult();
        }

        [HttpPost("leave")]
        public async Task<IActionResult> LeaveStudyGroup(int studyGroupId, int userId)
        {
            await _studyGroupRepository.LeaveStudyGroup(studyGroupId, userId);
            return new OkResult();
        }
    }
}