using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace LeaveManagement.API.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

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

        [StringLength(100)]
        public string Department { get; set; }

        [StringLength(100)]
        public string Designation { get; set; }

        [StringLength(15)]
        public string ContactNo { get; set; }

        [Required]
        [StringLength(255)]
        public string Password { get; set; }

        [Required]
        public int RoleId { get; set; }

        [ForeignKey("RoleId")]
        public Role Role { get; set; }

        public ICollection<Leave> Leaves { get; set; }
        public ICollection<LeaveBalance> LeaveBalances { get; set; }

        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";

        [StringLength(500)]
        public string? ProfilePicture { get; set; } // Stores file path or base64 string

        [NotMapped]
        public string ProfilePictureUrl
        {
            get
            {
                if (!string.IsNullOrEmpty(ProfilePicture))
                {
                    if (ProfilePicture.StartsWith("http") || ProfilePicture.StartsWith("data:"))
                        return ProfilePicture;
                    return ProfilePicture; // Return relative path, Angular will handle the base URL
                }

                // Return initials avatar as data URL
                var initials = $"{FirstName?.FirstOrDefault()}{LastName?.FirstOrDefault()}";
                // This should call a service to generate the avatar
                return GenerateInitialsAvatar(initials);
            }
        }

        [NotMapped]
        public string DefaultAvatarUrl => GetDefaultAvatarUrl();

        private string GetDefaultAvatarUrl()
        {
            // Generate initials-based avatar or use default image
            var initials = $"{FirstName?.FirstOrDefault()}{LastName?.FirstOrDefault()}";
            return $"data:image/svg+xml;base64,{GenerateInitialsAvatar(initials)}";
        }

        private string GenerateInitialsAvatar(string initials)
        {
            // This will be implemented in a service
            return string.Empty;
        }
    }
}