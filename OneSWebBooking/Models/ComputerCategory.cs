using System.ComponentModel.DataAnnotations;

namespace OneSWebBooking.Models
{
    public class ComputerCategory
    {
        [Key]
        public int Id { get; set; }     

        [Required(ErrorMessage = "Category name cannot be empty")]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Status")]
        public bool Status { get; set; } = true; // True: Active, False: Inactive
    }
}