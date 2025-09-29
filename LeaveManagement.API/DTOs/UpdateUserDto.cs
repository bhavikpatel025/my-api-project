using LeaveManagement.API.DTOs;
using System.ComponentModel.DataAnnotations;

public class UpdateUserDto
{
    [Required]
    [StringLength(50)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(50)]
    public string LastName { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(100)]
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

    public List<LeaveBalanceDto> LeaveBalances { get; set; } = new List<LeaveBalanceDto>();
}