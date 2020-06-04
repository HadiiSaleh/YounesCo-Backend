using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YounesCo_Backend.ViewModels
{
    public class ColorsForDetailsViewModel
    {
        public int ColorId { get; set; }

        public string ColorName { get; set; }

        public string ColorCode { get; set; }

        public bool Default { get; set; }

        public bool Deleted { get; set; } = false;

    }
}
