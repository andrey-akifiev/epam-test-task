using EPAM.StudyGroups.Data.Models;

namespace EPAM.StudyGroups.Data.DAL
{
    public interface IStudyGroupRepository
    {
        Task CreateStudyGroup(StudyGroup studyGroup);
        Task<IEnumerable<StudyGroup>> GetStudyGroups();
        Task JoinStudyGroup(int studyGroupId, int userId);
        Task LeaveStudyGroup(int studyGroupId, int userId);
        Task<IEnumerable<StudyGroup>> SearchStudyGroups(string subject);
    }
}