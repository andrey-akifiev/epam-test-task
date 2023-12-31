﻿using EPAM.StudyGroups.Api.Models;
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
        public async Task<IActionResult> CreateStudyGroup([FromBody] CreateStudyGroupRequest studyGroup, CancellationToken ctn)
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

        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchStudyGroups([FromQuery] SearchStudyGroupsRequest request, CancellationToken ctn)
        {
            var studyGroups = await _studyGroupRepository
                .SearchStudyGroups(request.Subject, ctn)
                .ConfigureAwait(false);

            return new OkObjectResult(studyGroups);
        }

        [HttpPut("join")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> JoinStudyGroup([FromQuery] JoinStudyGroupRequest request, CancellationToken ctn)
        {
            var studyGroup = (await _studyGroupRepository
                    .GetStudyGroups(ctn)
                    .ConfigureAwait(false))
                    .FirstOrDefault(g => g.StudyGroupId == request.StudyGroupId);

            var user = (await _userRepository
                    .GetUsers(ctn)
                    .ConfigureAwait(false))
                    .FirstOrDefault(u => u.Id == request.UserId);

            if (studyGroup == null)
            {
                return new NotFoundResult();
            }

            if (user == null)
            {
                return new NotFoundResult();
            }

            if (studyGroup.Users?.FirstOrDefault(u => u.Id == user.Id) != null)
            {
                return new ConflictResult();
            }

            await _studyGroupRepository
                .JoinStudyGroup(request.StudyGroupId, request.UserId, ctn)
                .ConfigureAwait(false);

            return new OkResult();
        }

        [HttpPut("leave")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LeaveStudyGroup([FromQuery] LeaveStudyGroupRequest request, CancellationToken ctn)
        {
            var studyGroup = (await _studyGroupRepository
                    .GetStudyGroups(ctn)
                    .ConfigureAwait(false))
                    .FirstOrDefault(g => g.StudyGroupId == request.StudyGroupId);

            var user = (await _userRepository
                    .GetUsers(ctn)
                    .ConfigureAwait(false))
                    .FirstOrDefault(u => u.Id == request.UserId);

            if (studyGroup == null)
            {
                return new NotFoundResult();
            }

            if (user == null)
            {
                return new NotFoundResult();
            }

            if (studyGroup.Users?.FirstOrDefault(u => u.Id == user.Id) == null)
            {
                return new NotFoundResult();
            }

            await _studyGroupRepository
                .LeaveStudyGroup(request.StudyGroupId, request.UserId, ctn)
                .ConfigureAwait(false);

            return new OkResult();
        }
    }
}