using MadeByMe.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace MadeByMe.DTOs
{
    public class CreatePostDto
    {
        [Required(ErrorMessage = "Поле 'Назва' є обов'язковим")]
        [MaxLength(100)]
        [Display(Name = "Заголовок")]
        public string? Title { get; set; }

        [Display(Name = "Опис")]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Ціна")]
        public decimal Price { get; set; }

        [Required]
        [Display(Name = "Фото")]
        public IFormFile? Photo { get; set; }

        [Required]
        [Display(Name = "Категорія")]
        public int CategoryId { get; set; }

        //public string SellerId { get; set; }
    }
}