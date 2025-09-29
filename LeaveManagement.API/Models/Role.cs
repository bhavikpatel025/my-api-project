using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.API.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public ICollection<User> Users { get; set; }
    }
}