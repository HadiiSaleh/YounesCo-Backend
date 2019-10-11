using System;
using System.ComponentModel.DataAnnotations;

namespace YounesCo_Backend.Models
{
    public class ExceptionLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "User Id")]
        [StringLength(450, ErrorMessage = "Maximum length is {1}")]
        public string UserId { get; set; }

        [Required]
        [StringLength(256, ErrorMessage = "Maximum length is {1}")]
        public string Source { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Maximum length is {1}")]
        public string Message { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
