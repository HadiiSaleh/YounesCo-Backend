using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace YounesCo_Backend.ViewModels
{
    public class ProductToListViewModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }

        [Display(Name = "Out Of Stock")]
        public bool OutOfStock{ get; set; }
        public bool Deleted { get; set; }

        public string ImageUrl { get; set; }
        public string DefaultColor { get; set; }
    }
}
