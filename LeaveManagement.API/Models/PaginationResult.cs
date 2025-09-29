namespace LeaveManagement.API.Models
{
    public class PaginationResult<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public PaginationInfo Pagination { get; set; } = new PaginationInfo();
    }

    public class PaginationInfo
    {
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
    }
}
