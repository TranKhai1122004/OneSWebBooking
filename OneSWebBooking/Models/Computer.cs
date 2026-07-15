using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OneSWebBooking.Models
{
    public class Computer
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên máy không được để trống")]
        [Display(Name = "Tên máy")]
        public string ComputerName { get; set; } = string.Empty;

        [Display(Name = "Tên quầy")]
        public string? CounterName { get; set; }

        [Display(Name = "Địa chỉ IP")]
        public string? IpAddress { get; set; }

        [Display(Name = "Địa chỉ vật lý")]
        public string? MacAddress { get; set; } // Đã đưa em nó trở lại đúng vị trí!

        [Display(Name = "Dịch vụ")]
        public string? ServiceName { get; set; }

        // Khóa ngoại liên kết tới ComputerCategory (Nhóm máy tính)
        [Display(Name = "Nhóm máy tính")]
        public int ComputerCategoryId { get; set; }
        [ForeignKey("ComputerCategoryId")]
        public ComputerCategory? ComputerCategory { get; set; }

        // Khóa ngoại liên kết tới Area (Khu vực)
        [Display(Name = "Khu vực")]
        public int AreaId { get; set; }
        [ForeignKey("AreaId")]
        public Area? Area { get; set; }

        [Display(Name = "Loại cổng")]
        public string? PortType { get; set; } // Ví dụ: Đi vào, Đi ra...

        [Display(Name = "Số lượng thẻ được nuốt trong thùng")]
        public int SwallowedCardCount { get; set; } = 0; // Số lượng thẻ nuốt trong thùng của máy ACM

        [Display(Name = "Trạng thái")]
        public bool Status { get; set; } = true; // True: Đang hoạt động, False: Không sử dụng

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