using System.ComponentModel.DataAnnotations;

namespace YounesCo_Backend.Models
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(256, ErrorMessage = "Maximum length is {1}")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(256, ErrorMessage = "Maximum length is {1}")]
        [RegularExpression("^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z0-9_]).*$")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Confirm password doesn't match!")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        [StringLength(256, ErrorMessage = "Maximum length is {1}")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.PhoneNumber)]
        [StringLength(256, ErrorMessage = "Maximum length is {1}")]
        [RegularExpression("[+][0-9]{3} [0-9]{8}")]
        public string Phone { get; set; }

        [Required]
        [StringLength(256, ErrorMessage = "Maximum length is {1}")]
        public string Role { get; set; }

        [Required]
        [StringLength(256, ErrorMessage = "Maximum length is {1}")]
        [RegularExpression("^[A-Za-z]+$")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [StringLength(256, ErrorMessage = "Maximum length is {1}")]
        [RegularExpression("^[A-Za-z]+$")]
        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        [Required]
        [StringLength(256, ErrorMessage = "Maximum length is {1}")]
        [RegularExpression("^[A-Za-z]+$")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [StringLength(256, ErrorMessage = "Maximum length is {1}")]
        [RegularExpression("^[A-Za-z]+$")]
        [Display(Name = "Name")]
        public string DisplayName { get; set; }

        [Required]
        public Address Location { get; set; }
    }
}
