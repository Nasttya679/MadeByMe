using System.ComponentModel.DataAnnotations;

namespace MadeByMe.DTOs
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Електронна пошта")]
        public string? Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string? Password { get; set; }
    }
}
