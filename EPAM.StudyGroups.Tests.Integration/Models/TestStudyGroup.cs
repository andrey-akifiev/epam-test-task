using EPAM.StudyGroups.Data.Models;

namespace EPAM.StudyGroups.Tests.Integration.Models
{
    public class TestStudyGroup : StudyGroup
    {
        public new List<User> Users { get; set; }
    }
}