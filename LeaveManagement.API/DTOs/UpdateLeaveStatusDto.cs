using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.API.DTOs
{
    public class UpdateLeaveStatusDto
    {
        [Required]
        public int LeaveId { get; set; }

        [Required]
        public string Status { get; set; } // Approved, Rejected
    }
}