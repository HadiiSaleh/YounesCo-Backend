using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using YounesCo_Backend.Models;

namespace YounesCo_Backend.ViewModels
{
    public class ProductDetailsViewModel
    {

        public int ProductId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int Quantity { get; set; }

        public double Price { get; set; }

        public int? CategoryId { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        public bool Deleted { get; set; } = false;

        public string Features { get; set; }

        public bool OutOfStock
        {
            get { return Quantity < 2; }
            set { }
        }

        public ColorsForListViewModel Color { get; set; }
        public List<ImageForListViewModel> Images { get; set; }



    }
}
