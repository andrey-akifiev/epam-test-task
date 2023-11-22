using EPAM.StudyGroups.Data.DAL;
using EPAM.StudyGroups.Data.Models;
using System.Collections.Concurrent;

namespace EPAM.StudyGroups.Tests.Integration.DAL
{
    public class TestStudyGroupRepository : IStudyGroupRepository
    {
        private int groupsCounter = 0;

        private ConcurrentDictionary<int, StudyGroup> studyGroups { get; init; } = new();

        private List<Tuple<int, int>> usersStudyGroups { get; init; } = new();

        public Task CreateStudyGroup(StudyGroup studyGroup, CancellationToken ctn)
        {
            studyGroup.StudyGroupId = ++this.groupsCounter;
            
            this.studyGroups.TryAdd(studyGroup.StudyGroupId.Value, studyGroup);

            return Task.CompletedTask;
        }

        public Task<IEnumerable<StudyGroup>> GetStudyGroups(CancellationToken ctn)
        {
            return Task.FromResult<IEnumerable<StudyGroup>>(this.studyGroups.Values.ToList());
        }

        public Task JoinStudyGroup(int studyGroupId, int userId, CancellationToken ctn)
        {
            this.usersStudyGroups.Add(new (studyGroupId, userId));

            return Task.CompletedTask;
        }

        public Task LeaveStudyGroup(int studyGroupId, int userId, CancellationToken ctn)
        {
            this.usersStudyGroups.RemoveAll(v => v.Item1 == studyGroupId && v.Item2 == userId);

            return Task.CompletedTask;
        }

        public Task<IEnumerable<StudyGroup>> SearchStudyGroups(string subject, CancellationToken ctn)
        {
            return Task.FromResult(
                this.studyGroups
                    .Where(g => g.Value.Subject == (Subject)Enum.Parse(typeof(Subject), subject))
                    .Select(g => g.Value));
        }
    }
}