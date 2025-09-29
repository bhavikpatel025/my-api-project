using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.API.DTOs
{
    public class ApplyLeaveDto
    {
        [Required]
        public int LeaveTypeId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(1000)]
        public string ReasonForLeave { get; set; }
    }
}