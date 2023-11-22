using EPAM.StudyGroups.Data.DAL;
using EPAM.StudyGroups.Data.Models;
using System.Collections.Concurrent;

namespace EPAM.StudyGroups.Tests.Integration.DAL
{
    public class TestUserRepository : IUserRepository
    {
        private int usersCounter = 0;
        private ConcurrentDictionary<int, User> users { get; init; } = new();

        public Task<IEnumerable<User>> GetUsers(CancellationToken ctn)
        {
            return Task.FromResult(this.users.Select(u => u.Value));
        }

        public void AddUser(User user)
        {
            user.Id = ++this.usersCounter;
            this.users.TryAdd(user.Id, user);
        }
    }
}