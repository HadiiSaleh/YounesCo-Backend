using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace YounesCo_Backend.Models
{
    [Owned]
    public class Address
    {
        [Required]
        [StringLength(256, ErrorMessage = "Maximum length for country is {1}")]
        public string Country { get; set; }

        [Required]
        [StringLength(256, ErrorMessage = "Maximum length for city is {1}")]
        public string City { get; set; }

        [Required]
        [StringLength(256, ErrorMessage = "Maximum length for region is {1}")]
        public string Region { get; set; }

        [Required]
        [StringLength(256, ErrorMessage = "Maximum length for street is {1}")]
        public string Street { get; set; }

        [Required]
        [StringLength(256, ErrorMessage = "Maximum length for building is {1}")]
        public string Building { get; set; }

        [Required]
        [StringLength(256, ErrorMessage = "Maximum length for floor is {1}")]
        public string Floor { get; set; }
    }
}
