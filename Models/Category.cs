using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace YounesCo_Backend.Models
{
    public class Category
    {
        [Key]
        [Display(Name = "Category Id")]
        public int CategoryId { get; set; }

        [Required]
        [Display(Name = "Category Name")]
        [StringLength(50, ErrorMessage = "Maximum length for category name is {1}")]
        public string CategoryName { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        [Required]
        public bool Deleted { get; set; } = false;

       public List<Product> Products { get; set; }
       // public List<Type> Types { get; set; }
    }
}
