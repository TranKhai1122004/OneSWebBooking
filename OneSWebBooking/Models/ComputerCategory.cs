using System.ComponentModel.DataAnnotations;

namespace OneSWebBooking.Models
{
    public class ComputerCategory
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name cannot be empty")]
        [Display(Name = "Tên nhóm máy")]
        public string CategoryName { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Trạng thái")]
        public bool Status { get; set; } = true; 

        // --- Các trường thông tin hệ thống (Audit Logs) ---
        [Display(Name = "Người tạo")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Người sửa")]
        public string? ModifiedBy { get; set; }

        [Display(Name = "Ngày sửa")]
        public DateTime? ModifiedDate { get; set; }
    }
}