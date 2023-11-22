using EPAM.StudyGroups.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EPAM.StudyGroups.Data.DAL
{
    public class StudyGroupRepository : IStudyGroupRepository, IDisposable
    {
        private readonly StudyGroupsContext context;

        private bool disposed = false;

        public StudyGroupRepository(StudyGroupsContext context)
        {
            this.context = context;
        }

        public async Task CreateStudyGroup(StudyGroup studyGroup)
        {
            await context
                .StudyGroups
                .AddAsync(studyGroup)
                .ConfigureAwait(false);
            await context
                .SaveChangesAsync()
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<StudyGroup>> GetStudyGroups()
        {
            return await this.context
                .StudyGroups
                .Include(g => g.Users)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task JoinStudyGroup(int studyGroupId, int userId)
        {
            (await this.context
                .StudyGroups
                .FindAsync(studyGroupId)
                .ConfigureAwait(false))
                .AddUser(await this.context
                    .Users
                    .FindAsync(userId)
                    .ConfigureAwait(false));
            await this.context
                .SaveChangesAsync()
                .ConfigureAwait(false);
        }

        public async Task LeaveStudyGroup(int studyGroupId, int userId)
        {
            (await this.context
                .StudyGroups
                .FindAsync(studyGroupId)
                .ConfigureAwait(false))
                .RemoveUser(await this.context
                    .Users
                    .FindAsync(userId)
                    .ConfigureAwait(false));
            await this.context
                .SaveChangesAsync()
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<StudyGroup>> SearchStudyGroups(string subject)
        {
            return await this.context
                .StudyGroups
                .Where(g => g.Subject == (Subject)Enum.Parse(typeof(Subject), subject))
                .ToListAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }

            this.disposed = true;
        }
    }
}
