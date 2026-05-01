using MadeByMe.Domain.Entities;

namespace MadeByMe.Application.ViewModels
{
    public class StorefrontViewModel
    {
        public string SellerId { get; set; } = string.Empty;

        public string? SellerName { get; set; }

        public string? SellerAvatar { get; set; }

        public string? SellerDescription { get; set; }

        public List<MadeByMe.Application.DTOs.PostResponseDto> Posts { get; set; } = new();
    }

    public class UpdateDescriptionDto
    {
        public string? Description { get; set; }
    }
}