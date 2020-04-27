using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace YounesCo_Backend.ViewModels
{
    public class CreateProductViewModel
    {
        [Required]
        [StringLength(256, ErrorMessage = "Maximum length for product name is {1}")]
        public string Name { get; set; }

        [Required]
        [StringLength(450, ErrorMessage = "Maximum length for product description is {1}")]
        public string Description { get; set; }

        [Range(0, 1000000000)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, 1000000000)]
        public double Price { get; set; }
    }
}
