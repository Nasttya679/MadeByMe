using MadeByMe.Models;

namespace MadeByMe.Models
{
    public class SellerPost
    {
        public string SellerId { get; set; } = null!;

        public ApplicationUser? Seller { get; set; }

        public int PostId { get; set; }

        public Post? Post { get; set; }
    }
}