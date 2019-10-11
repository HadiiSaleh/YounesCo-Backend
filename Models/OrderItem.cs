using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YounesCo_Backend.Models
{
    public class OrderItem
    {
        [Key]
        [Display(Name = "Order Item Id")]
        public int OrderItemId { get; set; }

        [Required]
        [Display(Name = "Product Id")]
        [Range(0, 1000000000)]
        public int ProductId { get; set; }

        [Required]
        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [Required]
        [Display(Name = "Order Id")]
        [Range(0, 1000000000)]
        public int OrderId { get; set; }

        [Required]
        [ForeignKey("OrderId")]
        public Order Order { get; set; }

        [Required]
        [Range(0, 1000000000)]
        public int Quantity { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Maximum length is {1}")]
        [Display(Name = "Color")]
        public string ColorName { get; set; }

        [Required]
        [Display(Name = "Unit Price")]
        [Range(0, 1000000000)]
        public double UnitPrice { get; set; }

        [Required]
        [Display(Name = "Total Price")]
        [Range(0, 1000000000)]
        public double TotalPrice { get; set; }

        [Required]
        [Display(Name = "Color Id")]
        [Range(0, 1000000000)]
        public int ColorId { get; set; }

        [Required]
        [ForeignKey("ColorId")]
        public Color Color { get; set; }
    }
}
