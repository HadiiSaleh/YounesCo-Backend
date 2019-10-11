using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YounesCo_Backend.Models
{
    public class Image
    {
        [Key]
        [Display(Name = "Image Id")]
        public int ImageId { get; set; }

        [Required]
        [Display(Name = "Color Id")]
        [Range(0, 1000000000)]
        public int ColorId { get; set; }

        [Required]
        [ForeignKey("ColorId")]
        public Color Color { get; set; }

        [Required]
        [Display(Name = "Image Source")]
        [StringLength(1000, ErrorMessage = "Maximum length is {1}")]
        public string ImageSource { get; set; }

        [Required]
        public bool Default { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }
    }
}
