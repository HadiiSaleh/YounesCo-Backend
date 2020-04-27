using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YounesCo_Backend.ViewModels
{
    public class AddTypeToCategoryViewModel
    {

        public int CategoryId { get; set; }
        public List<TypesForListViewModel> Types { get; set; }
    }
}
