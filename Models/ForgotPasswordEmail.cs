using System.ComponentModel.DataAnnotations;

namespace YounesCo_Backend.Models
{
    public class ForgotPasswordEmail
    {
        [Required]
        [StringLength(256, ErrorMessage = "Maximum length for email address is {1}")]
        [DataType(DataType.EmailAddress)]
        public string email { get; set; }
    }
}
