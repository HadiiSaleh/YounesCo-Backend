using System.ComponentModel.DataAnnotations;

namespace YounesCo_Backend.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required]
        [StringLength(400, ErrorMessage = "Maximum length for id is {1}")]
        [Display(Name = "Id")]
        public string Id { get; set; }

        [Required]
        [StringLength(400, ErrorMessage = "Maximum length for code is {1}")]
        [Display(Name = "Code")]
        public string Code { get; set; }

        [Required]
        [StringLength(256, ErrorMessage = "Maximum length for password is {1}")]
        [RegularExpression("^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z0-9_]).*$")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "Confirm password doesn't match!")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public ResetPasswordViewModel() { }

        public ResetPasswordViewModel(string id,
        string code,
        string password,
        string ConfirmPassword)
        {
            this.Id = id;
            this.Code = code;
            this.Password = password;
            this.ConfirmPassword = ConfirmPassword;
        }
    }
}
