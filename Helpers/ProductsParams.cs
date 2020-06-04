using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YounesCo_Backend.Helpers
{
    public class ProductsParams : PagedParams
    {
        public string CategoriesId { get; set; }

        public double? MinPrice { get; set; } = 0;
        public double? MaxPrice { get; set; }
        public string OrderByPrice { get; set; }

        public string Search { get; set; }

        public string ColorsId { get; set; }
        //public double Popularity { get; set; }
    }
}
