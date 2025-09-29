using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeaveManagement.API.Models
{
    public class LeaveBalance
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int LeaveTypeId { get; set; }

        [Required]
        public int Balance { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("LeaveTypeId")]
        public LeaveType LeaveType { get; set; }
    }
}