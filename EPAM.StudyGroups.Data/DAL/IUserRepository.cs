using EPAM.StudyGroups.Data.Models;

namespace EPAM.StudyGroups.Data.DAL
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetUsers(CancellationToken ctn);
    }
}