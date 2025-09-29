using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeaveManagement.API.Models
{
    public class Leave
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int LeaveTypeId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public DateTime DateOfRequest { get; set; }

        [Required]
        [StringLength(1000)]
        public string ReasonForLeave { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Cancelled

        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("LeaveTypeId")]
        public LeaveType LeaveType { get; set; }

        [NotMapped]
        public int LeaveDays => (EndDate - StartDate).Days + 1;
    }

    public enum LeaveStatus
    {
        Pending,
        Approved,
        Rejected,
        Cancelled
    }
}