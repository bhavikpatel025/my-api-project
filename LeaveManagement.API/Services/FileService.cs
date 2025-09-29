using System.Text;

namespace LeaveManagement.API.Services
{
    public interface IFileService
    {
        Task<string> UploadProfilePictureAsync(IFormFile file, int userId);
        Task<bool> DeleteProfilePictureAsync(string fileName);
        string GenerateInitialsAvatar(string initials, string backgroundColor = null);
        bool IsValidImageFile(IFormFile file);
    }
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileService> _logger;
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

        public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string> UploadProfilePictureAsync(IFormFile file, int userId)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file provided");

            if (!IsValidImageFile(file))
                throw new ArgumentException("Invalid image file");

            if (file.Length > _maxFileSize)
                throw new ArgumentException($"File size exceeds maximum limit of {_maxFileSize / (1024 * 1024)}MB");

            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            // Generate unique filename
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"user_{userId}_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save file
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Return relative path for database storage
            return $"/uploads/profiles/{fileName}";
        }

        public async Task<bool> DeleteProfilePictureAsync(string fileName)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                    return false;

                // Extract filename from path
                var fileNameOnly = Path.GetFileName(fileName);
                var filePath = Path.Combine(_environment.WebRootPath, "uploads", "profiles", fileNameOnly);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting profile picture: {FileName}", fileName);
                return false;
            }
        }

        public string GenerateInitialsAvatar(string initials, string backgroundColor = null)
        {
            if (string.IsNullOrEmpty(initials))
                initials = "U";

            if (initials.Length > 2)
                initials = initials.Substring(0, 2);

            backgroundColor ??= GenerateColorFromText(initials);

            var svg = $@"
            <svg width='100' height='100' xmlns='http://www.w3.org/2000/svg'>
                <rect width='100' height='100' fill='{backgroundColor}' />
                <text x='50' y='50' font-family='Arial, sans-serif' font-size='40' 
                      font-weight='bold' fill='white' text-anchor='middle' 
                      dominant-baseline='central'>{initials.ToUpper()}</text>
            </svg>";

            var svgBytes = Encoding.UTF8.GetBytes(svg);
            return Convert.ToBase64String(svgBytes);
        }

        public bool IsValidImageFile(IFormFile file)
        {
            if (file == null) return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return false;

            // Check MIME type
            var validMimeTypes = new[] { "image/jpeg", "image/png", "image/gif" };
            if (!validMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                return false;

            return true;
        }

        private string GenerateColorFromText(string text)
        {
            var colors = new[]
            {
            "#FF6B6B", "#4ECDC4", "#45B7D1", "#96CEB4", "#FECA57",
            "#FF9FF3", "#54A0FF", "#5F27CD", "#00D2D3", "#FF9F43",
            "#F368E0", "#FE4A49", "#01A3A4", "#2E86AB", "#A23E48"
        };

            var hash = text.GetHashCode();
            var index = Math.Abs(hash) % colors.Length;
            return colors[index];
        }
    }
}
