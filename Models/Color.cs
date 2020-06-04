using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YounesCo_Backend.Models
{
    public class Color
    {
        [Key]
        [Display(Name = "Color Id")]
        public int ColorId { get; set; }

      //  [Required]
       // [Range(0, 1000000000)]
       // [Display(Name = "Product Id")]
       // public int? ProductId { get; set; }

       // [ForeignKey("ProductId")]
      //  public Product Product { get; set; }

        public List<Product> Products { get; set; }

        [Required]
        [Display(Name = "Color Name")]
        [StringLength(50, ErrorMessage = "Maximum length for color name is {1}")]
        public string ColorName { get; set; }

        [Required]
        [Display(Name = "Color Code")]
        [StringLength(50, ErrorMessage = "Maximum length for color code is {1}")]
        public string ColorCode { get; set; }

        [Required]
        public bool Default { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        [Required]
        public bool Deleted { get; set; } = false;

       // public List<Image> Images { get; set; }

       // public List<OrderItem> OrderItems { get; set; }
    }
}
