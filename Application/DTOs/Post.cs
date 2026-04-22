using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MadeByMe.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace MadeByMe.Application.DTOs
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

        // public string SellerId { get; set; }
    }

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

    public class PostResponseDto
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string? PhotoUrl { get; set; }

        public decimal Rating { get; set; }

        public string? Status { get; set; }

        public Category? CategoryName { get; set; }

        public ApplicationUser? SellerName { get; set; }

        public string? SellerId { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsFavorite { get; set; } = false;

        public bool IsDeleted { get; set; } = false;
    }
}