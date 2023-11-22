using EPAM.StudyGroups.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace EPAM.StudyGroups.Api.Models
{
    public class CreateStudyGroupRequest
    {
        [Required]
        [MinLength(5)]
        [MaxLength(30)]
        public string Name { get; set; }

        [Required]
        public Subject Subject { get; set; }
    }
}