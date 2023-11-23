using EPAM.StudyGroups.Data;
using EPAM.StudyGroups.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EPAM.StudyGroups.Tests.Integration.DAL
{
    public class DevelopmentTestUserRepository : ITestUserRepository
    {
        private readonly string dbConnectionString;

        public DevelopmentTestUserRepository(string dbConnectionString)
        {
            this.dbConnectionString = dbConnectionString ?? throw new ArgumentNullException(nameof(dbConnectionString));
        }

        public void AddUser(User user)
        {
            using StudyGroupsContext context = this.GetContext();
            context.Users.Add(user);
            context.SaveChanges();
        }

        public async Task<IEnumerable<User>> GetUsers(CancellationToken ctn)
        {
            using StudyGroupsContext context = this.GetContext();
            return await context.Users.ToListAsync().ConfigureAwait(false);
        }

        private StudyGroupsContext GetContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<StudyGroupsContext>();
            optionsBuilder.UseSqlServer(dbConnectionString);

            return new StudyGroupsContext(optionsBuilder.Options);
        }
    }
}