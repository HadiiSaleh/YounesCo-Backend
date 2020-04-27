using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace YounesCo_Backend.ViewModels
{
    public class CategoryToUpdateViewModel
    {
        [Required]
        [Display(Name = "Category Name")]
        [StringLength(50, ErrorMessage = "Maximum length for category name is {1}")]
        public string CategoryName { get; set; }

        [Required]
        public bool Deleted { get; set; } = false;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
