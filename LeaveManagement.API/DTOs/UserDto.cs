namespace LeaveManagement.API.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public string ContactNo { get; set; }
        public string Role { get; set; }
        public List<LeaveBalanceDto> LeaveBalances { get; set; } = new List<LeaveBalanceDto>();

        public string ProfilePictureUrl { get; set; }
    }
}