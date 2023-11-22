using EPAM.StudyGroups.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace EPAM.StudyGroups.Data
{
    public class StudyGroupsContext : DbContext
    {
        public StudyGroupsContext(DbContextOptions<StudyGroupsContext> options)
            : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; }

        public virtual DbSet<StudyGroup> StudyGroups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<User>()
                .ToTable($"{nameof(User)}s");

            modelBuilder
                .Entity<User>()
                .HasKey(u => u.Id);

            modelBuilder
                .Entity<User>()
                .Property(u => u.Email)
                .HasMaxLength(250)
                .IsRequired();

            modelBuilder
                .Entity<User>()
                .Property(u => u.FirstName)
                .HasMaxLength(50);

            modelBuilder
                .Entity<User>()
                .Property(u => u.LastName)
                .HasMaxLength(50);

            modelBuilder
                .Entity<StudyGroup>()
                .ToTable($"{nameof(StudyGroup)}s");

            modelBuilder
                .Entity<StudyGroup>()
                .HasKey(g => g.StudyGroupId);

            modelBuilder
                .Entity<StudyGroup>()
                .Property(g => g.Name)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder
                .Entity<StudyGroup>()
                .Property(g => g.Subject)
                .IsRequired();

            modelBuilder
                .Entity<StudyGroup>()
                .Property(g => g.CreateDate);

            modelBuilder
                .Entity<StudyGroup>()
                .HasMany(g => g.Users)
                .WithMany(u => u.StudyGroups)
                .UsingEntity(t => t.ToTable($"{nameof(User)}s{nameof(StudyGroup)}s"));
        }
    }
}