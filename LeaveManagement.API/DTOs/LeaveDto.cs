namespace LeaveManagement.API.DTOs
{
    public class LeaveDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int LeaveTypeId { get; set; }
        public string LeaveTypeName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime DateOfRequest { get; set; }
        public string ReasonForLeave { get; set; }
        public string Status { get; set; }
        public int LeaveDays { get; set; }
    }
}