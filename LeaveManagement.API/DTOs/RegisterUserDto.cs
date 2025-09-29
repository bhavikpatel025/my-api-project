using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.API.DTOs
{
    public class RegisterUserDto
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        [StringLength(100)]
        public string Department { get; set; }

        [Required]
        [StringLength(100)]
        public string Designation { get; set; }

        [Required]
        [StringLength(15)]
        public string ContactNo { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; }

        public List<LeaveBalanceDto> LeaveBalances { get; set; } = new List<LeaveBalanceDto>();
    }
}