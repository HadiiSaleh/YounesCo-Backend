using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using YounesCo_Backend.Models;

namespace YounesCo_Backend.ViewModels
{
    public class CreateProductViewModel
    {
        [Required]
        [StringLength(256, ErrorMessage = "Maximum length for product name is {1}")]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [Range(0, 1000000000)]
        public int Quantity { get; set; }

        [Required]
        [Range(0, 1000000000)]
        public double Price { get; set; }

        [Required]
        public string ColorName { get; set; }

        [Required]
        public string CategoryName { get; set; }
        public bool OutOfStock { get; set; } = true;
        public string Features { get; set; }
    }
}
