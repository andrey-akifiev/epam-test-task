using Microsoft.AspNetCore.Mvc;

namespace EPAM.StudyGroups.Api.Models
{
    public class SearchStudyGroupsRequest
    {
        [FromQuery(Name = nameof(Subject))]
        public string Subject { get; set; }
    }
}