using System.ComponentModel.DataAnnotations;

namespace YounesCo_Backend.Models
{
    public class LoginViewModel
    {
        [Required]
        [StringLength(256, ErrorMessage = "Maximum length is {1}")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(256, ErrorMessage = "Maximum length is {1}")]
        [RegularExpression("^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z0-9_]).*$")]
        public string Password { get; set; }
    }
}
