using EPAM.StudyGroups.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EPAM.StudyGroups.Data
{
    public class StudyGroupsContext : DbContext
    {
        public StudyGroupsContext()
        {
        }

        public StudyGroupsContext(DbContextOptions<StudyGroupsContext> options)
            : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<StudyGroup> StudyGroups { get; set; }
    }
}