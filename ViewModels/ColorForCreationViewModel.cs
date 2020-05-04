using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace YounesCo_Backend.ViewModels
{
    public class ColorForCreationViewModel
    {
        [Required]
        public string ColorName { get; set; }

        [Required]
        public string ColorCode { get; set; }

        [Required]
        public bool Default { get; set; }
    }
}
