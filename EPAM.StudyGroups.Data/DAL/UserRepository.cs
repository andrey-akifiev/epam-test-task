using EPAM.StudyGroups.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EPAM.StudyGroups.Data.DAL
{
    public class UserRepository : IUserRepository, IDisposable
    {
        private readonly StudyGroupsContext context;

        private bool disposed = false;

        public UserRepository(StudyGroupsContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<User>> GetUsers(CancellationToken ctn)
        {
            return await this.context
                .Users
                .Include(g => g.StudyGroups)
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