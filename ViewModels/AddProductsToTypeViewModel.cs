using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YounesCo_Backend.ViewModels
{
    public class AddProductsToTypeViewModel
    {
        public List<int> ProductsIds { get; set; }

        public int TypeId { get; set; }
    }
}
