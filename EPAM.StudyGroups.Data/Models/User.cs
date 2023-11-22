using System.ComponentModel.DataAnnotations;

namespace EPAM.StudyGroups.Data.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        public string FirstName { get; set; }

        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [StringLength(250)]
        public string Email { get; set; }

        public IEnumerable<StudyGroup> StudyGroups { get; set; }
    }
}