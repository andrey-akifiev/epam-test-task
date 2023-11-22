using EPAM.StudyGroups.Data.Models;

namespace EPAM.StudyGroups.Api.Models
{
    public class CreateStudyGroupRequest
    {
        public int Id { get; internal set; }

        public string Name { get; set; }

        public Subject Subject { get; set; }
    }
}