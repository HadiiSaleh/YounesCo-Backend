using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace YounesCo_Backend.Models
{
    public class Type
    {
        [Key]
        [Display(Name = "Type Id")]
        public int TypeId { get; set; }

        [Required]
        [Display(Name = "Type Name")]
        [StringLength(50, ErrorMessage = "Maximum length for type name is {1}")]
        public string TypeName { get; set; }

        [Range(0, 1000000000)]
        [Display(Name = "Category Id")]
        public int? CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        public List<Product> Products { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        [Required]
        public bool Deleted { get; set; }


    }
}
