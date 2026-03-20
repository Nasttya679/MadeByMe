using MadeByMe.Models;

namespace MadeByMe.Models
{
    public class Post
    {
        public int Id { get; set; }

        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public string SellerId { get; set; } = null!;
        public ApplicationUser? Seller { get; set; }

        public ICollection<Comment> CommentsList { get; set; } = new List<Comment>();
        public ICollection<Photo> Photos { get; set; } = new List<Photo>();
        public ICollection<SellerPost> SellerPosts { get; set; } = new List<SellerPost>();
        public ICollection<BuyerCart> BuyerCarts { get; set; } = new List<BuyerCart>();
    }
}