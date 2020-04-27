using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace YounesCo_Backend.ViewModels
{
    public class TypesForListViewModel
    {
        [Display(Name = "Type Id")]
        public int TypeId { get; set; }

        [Display(Name = "Type Name")]
        [StringLength(50, ErrorMessage = "Maximum length for type name is {1}")]
        public string TypeName { get; set; }
    }
}
