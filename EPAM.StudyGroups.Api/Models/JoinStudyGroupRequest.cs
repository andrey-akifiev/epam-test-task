using Microsoft.AspNetCore.Mvc;

namespace EPAM.StudyGroups.Api.Models
{
    public class JoinStudyGroupRequest
    {
        [FromQuery(Name = nameof(StudyGroupId))]
        public int StudyGroupId { get; set; }

        [FromQuery(Name = nameof(UserId))]
        public int UserId { get; set; }
    }
}