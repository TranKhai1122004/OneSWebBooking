using System.ComponentModel.DataAnnotations;

namespace OneSWebBooking.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Username cannot be empty")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email cannot be empty")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        // Lưu mật khẩu đã được HASH (Không bao giờ lưu mật khẩu text thường vào DB)
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        // Salt dùng để bảo mật thêm khi hash mật khẩu
        [Required]
        public string PasswordSalt { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; } = "User"; // Ví dụ: Admin, Manager, User

        [Display(Name = "Full Name")]
        public string? FullName { get; set; }

        [Display(Name = "Status")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}