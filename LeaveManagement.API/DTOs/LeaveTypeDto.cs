namespace LeaveManagement.API.DTOs
{

    public class LeaveTypeDto
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
    }
}