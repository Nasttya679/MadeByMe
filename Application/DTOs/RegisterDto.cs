using System.ComponentModel.DataAnnotations;

namespace MadeByMe.Application.DTOs
{
    public class RegisterDto
    {
        [Required]
        [Display(Name = "Ім'я користувача")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Електронна пошта")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Номер мобільного")]
        public string PhoneNumber { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        [Display(Name = "Підтвердження паролю")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Адреса")]
        public string Address { get; set; }
    }

}
