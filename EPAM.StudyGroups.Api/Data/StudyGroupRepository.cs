using EPAM.StudyGroups.Api.Models;

namespace EPAM.StudyGroups.Api.Data
{
    public class StudyGroupRepository : IStudyGroupRepository
    {
        public Task CreateStudyGroup(StudyGroup studyGroup)
        {
            return Task.CompletedTask;
        }

        public Task<IEnumerable<StudyGroup>> GetStudyGroups()
        {
            return Task.FromResult<IEnumerable<StudyGroup>>(Array.Empty<StudyGroup>());
        }

        public Task JoinStudyGroup(int studyGroupId, int userId)
        {
            return Task.CompletedTask;
        }

        public Task LeaveStudyGroup(int studyGroupId, int userId)
        {
            return Task.CompletedTask;
        }

        public Task<IEnumerable<StudyGroup>> SearchStudyGroups(string subject)
        {
            return Task.FromResult<IEnumerable<StudyGroup>>(Array.Empty<StudyGroup>());
        }
    }
}
