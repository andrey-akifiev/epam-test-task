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

        public async Task CreateStudyGroup(StudyGroup studyGroup, CancellationToken ctn)
        {
            await context
                .StudyGroups
                .AddAsync(studyGroup, ctn)
                .ConfigureAwait(false);
            await context
                .SaveChangesAsync(ctn)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<StudyGroup>> GetStudyGroups(CancellationToken ctn)
        {
            return await this.context
                .StudyGroups
                .Include(g => g.Users)
                .ToListAsync(ctn)
                .ConfigureAwait(false);
        }

        public async Task JoinStudyGroup(int studyGroupId, int userId, CancellationToken ctn)
        {
            (await this.context
                .StudyGroups
                .FindAsync(new object[] { studyGroupId }, cancellationToken: ctn)
                .ConfigureAwait(false))
                .AddUser(await this.context
                    .Users
                    .FindAsync(new object[] { userId }, cancellationToken: ctn)
                    .ConfigureAwait(false));
            await this.context
                .SaveChangesAsync(ctn)
                .ConfigureAwait(false);
        }

        public async Task LeaveStudyGroup(int studyGroupId, int userId, CancellationToken ctn)
        {
            (await this.context
                .StudyGroups
                .FindAsync(new object[] { studyGroupId }, cancellationToken: ctn)
                .ConfigureAwait(false))
                .RemoveUser(await this.context
                    .Users
                    .FindAsync(new object[] { userId }, cancellationToken: ctn)
                    .ConfigureAwait(false));
            await this.context
                .SaveChangesAsync(ctn)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<StudyGroup>> SearchStudyGroups(string subject, CancellationToken ctn)
        {
            return await this.context
                .StudyGroups
                .Where(g => g.Subject == (Subject)Enum.Parse(typeof(Subject), subject))
                .ToListAsync(ctn)
                .ConfigureAwait(false);
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