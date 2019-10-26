using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using YounesCo_Backend.Data;

namespace YounesCo_Backend.Models
{
    public class Order
    {
        [Key]
        [Display(Name = "Order Id")]
        public int OrderId { get; set; }

        [Required]
        [Display(Name = "Customer Id")]
        [StringLength(450, ErrorMessage = "Maximum length for customer id is {1}")]
        public string CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public AppUser Customer { get; set; }

        [Display(Name = "Requested On")]
        [MaxLength(100)]
        public DateTime? RequestedOn { get; set; }

        [Required]
        [Display(Name = "Total Price")]
        [Range(0, 1000000000)]
        public double TotalPrice { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        [Required]
        public bool Deleted { get; set; }

        public List<OrderItem> OrderItems { get; set; }
    }
}
