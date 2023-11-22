using EPAM.StudyGroups.Data.Models;

namespace EPAM.StudyGroups.Data.DAL
{
    public interface IStudyGroupRepository
    {
        Task CreateStudyGroup(StudyGroup studyGroup, CancellationToken ctn);

        Task<IEnumerable<StudyGroup>> GetStudyGroups(CancellationToken ctn);

        Task JoinStudyGroup(int studyGroupId, int userId, CancellationToken ctn);

        Task LeaveStudyGroup(int studyGroupId, int userId, CancellationToken ctn);

        Task<IEnumerable<StudyGroup>> SearchStudyGroups(string subject, CancellationToken ctn);
    }
}