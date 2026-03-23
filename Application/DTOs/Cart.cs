using System.ComponentModel.DataAnnotations;

namespace MadeByMe.Application.DTOs
{
    public class AddToCartDto
    {

        [Required]
        public int PostId { get; set; }

        [Range(1, 100)]
        public int Quantity { get; set; } = 1;
    }

    public class CartItemResponseDto
    {
        public int CartItemId { get; set; }
        public int PostId { get; set; }
        public string PostTitle { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => Price * Quantity;
    }

    public class CartResponseDto
    {
        public int CartId { get; set; }
        public List<CartItemResponseDto> Items { get; set; } = new();
        public decimal TotalPrice => Items.Sum(i => i.TotalPrice);
    }
}