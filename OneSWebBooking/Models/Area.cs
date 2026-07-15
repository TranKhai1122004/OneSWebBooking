using System.ComponentModel.DataAnnotations;

namespace OneSWebBooking.Models
{
    public class Area
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Area name cannot be empty")]
        [Display(Name = "Area Name")]
        public string AreaName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }
    }
}