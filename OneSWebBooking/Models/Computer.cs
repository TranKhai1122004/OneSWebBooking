using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OneSWebBooking.Models
{
    public class Computer
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Computer name cannot be empty")]
        [Display(Name = "Computer Name")]
        public string ComputerName { get; set; } = string.Empty;

        [Display(Name = "Counter Name")]
        public string? CounterName { get; set; }

        [Display(Name = "IP Address")]
        public string? IpAddress { get; set; }

        [Display(Name = "Mac Address")]
        public string? MacAddress { get; set; }

        // Foreign Key to ComputerCategory
        [Display(Name = "Computer Category")]
        public int ComputerCategoryId { get; set; }
        [ForeignKey("ComputerCategoryId")]
        public ComputerCategory? ComputerCategory { get; set; }

        // Foreign Key to Area
        [Display(Name = "Area Location")]
        public int AreaId { get; set; }
        [ForeignKey("AreaId")]
        public Area? Area { get; set; }

        [Display(Name = "Status")]
        public bool Status { get; set; } = true;
    }
}