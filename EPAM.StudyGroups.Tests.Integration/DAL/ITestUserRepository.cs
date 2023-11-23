using EPAM.StudyGroups.Data.DAL;
using EPAM.StudyGroups.Data.Models;

namespace EPAM.StudyGroups.Tests.Integration.DAL
{
    public interface ITestUserRepository : IUserRepository
    {
        void AddUser(User user);
    }
}