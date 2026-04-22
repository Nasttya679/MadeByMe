namespace MadeByMe.Application.DTOs
{
    public class WishlistItemDto
    {
        public int PostId { get; set; }

        public string Title { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string PhotoUrl { get; set; } = string.Empty;

        public DateTime AddedAt { get; set; }
    }
}