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

        // [Required]
        [Display(Name = "Color Id")]
        [Range(0, 1000000000)]
        public int? ColorId { get; set; }

        [ForeignKey("ColorId")]
        public Color Color { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        public string ImageName { get; set; }

        [Required]
        [Display(Name = "Image Source")]
        [StringLength(1000, ErrorMessage = "Maximum length for image source is {1}")]
        public string ImageSource { get ; set; }

        private string ImageBaseUrl = "https://localhost:44364/";

        [NotMapped]
        public string ImageUrl
        {
            get { return ImageBaseUrl = "https://localhost:44364/" + ImageSource; }
        }


        [Required]
        public bool Default { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        public bool Deleted { get; set; } = false;
    }
}
