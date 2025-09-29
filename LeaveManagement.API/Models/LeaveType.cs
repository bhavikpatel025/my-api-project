using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.API.Models
{
    public class LeaveType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Type { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }

        public ICollection<Leave> Leaves { get; set; }
        public ICollection<LeaveBalance> LeaveBalances { get; set; }
    }
}