using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YounesCo_Backend.Data;

namespace YounesCo_Backend.Models
{
    public class Product
    {
        [Key]
        [Display(Name = "Product Id")]
        public int ProductId { get; set; }

        [Required]
        [StringLength(256, ErrorMessage = "Maximum length for product name is {1}")]
        public string Name { get; set; }

        [Required]
        [StringLength(450, ErrorMessage = "Maximum length for product description is {1}")]
        public string Description { get; set; }

        [Required]
        [Range(0, 1000000000)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, 1000000000)]
        public double Price { get; set; }

        /* [Required]
         [Display(Name = "Category Id")]
         [Range(0, 1000000000)]
         public int CategoryId { get; set; }

         [ForeignKey("CategoryId")]
         public Category Category { get; set; }*/

        [Display(Name = "Type Id")]
        [Range(0, 1000000000)]
        public int? TypeId { get; set; }

        [ForeignKey("TypeId")]
        public Type Type { get; set; }


        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        [Required]
        public bool Deleted { get; set; } = false;

        [Required]
        [Display(Name = "Out Of Stock")]
        public bool OutOfStock
        {
            get { return Quantity < 5; }
            set { }
        }

        public List<Color> Colors { get; set; }

        public List<OrderItem> OrderItems { get; set; }

        public List<Favorite> Favorites { get; set; }
    }
}
