using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MadeByMe.DTOs
{
    public class UpdateProfileDto
    {
        public string? UserId { get; set; }
        [Required]
        [Display(Name = "Ім'я користувача")]
        public string? UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Електронна пошта")]
        public string? Email { get; set; }

        [MaxLength(50)]
        [Display(Name = "Номер мобільного")]
        public string? PhoneNumber { get; set; }

        [MaxLength(255)]
        [Display(Name = "Аватар")]
        public string? ProfilePicture { get; set; }

        [MaxLength(255)]
        [Display(Name = "Адреса")]
        public string? Address { get; set; }
    }
}
