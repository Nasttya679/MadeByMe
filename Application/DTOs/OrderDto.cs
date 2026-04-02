using System.ComponentModel.DataAnnotations;

namespace MadeByMe.Application.DTOs
{
    public class OrderDto
    {
        [Required(ErrorMessage = "Ім'я обов'язкове")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Прізвище обов'язкове")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Email обов'язковий")]
        [EmailAddress(ErrorMessage = "Некоректний формат Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Номер телефону обов'язковий")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Місто обов'язкове")]
        public string? City { get; set; }

        [Required(ErrorMessage = "Відділення пошти обов'язкове")]
        public string? PostOffice { get; set; }

        [Required(ErrorMessage = "Введіть номер карти")]
        public string? CardNumber { get; set; }

        [Required(ErrorMessage = "Введіть термін дії")]
        public string? ExpiryDate { get; set; }

        [Required(ErrorMessage = "Введіть CVV")]
        public string? CVV { get; set; }
    }
}