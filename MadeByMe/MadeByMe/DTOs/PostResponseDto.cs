using MadeByMe.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace MadeByMe.DTOs
{
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
        public DateTime CreatedAt { get; set; }
    }
}