using MadeByMe.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace MadeByMe.DTOs
{
    public class UpdatePostDto
    {
        [MaxLength(100)]
        [Display(Name = "Заголовок")]
        public string? Title { get; set; }

        [Display(Name = "Опис")]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Ціна")]
        public decimal? Price { get; set; }

        [Display(Name = "Фото")]
        public IFormFile? Photo { get; set; }

        [Display(Name = "Категорія")]
        public int? CategoryId { get; set; }
    }
}