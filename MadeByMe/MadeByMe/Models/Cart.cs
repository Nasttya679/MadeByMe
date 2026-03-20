using MadeByMe.Models;

namespace MadeByMe.Models
{
    public class Cart
    {
        public int Id { get; set; }

        public string BuyerId { get; set; } = null!;
        public ApplicationUser? Buyer { get; set; }

        public ICollection<BuyerCart> BuyerCarts { get; set; } = new List<BuyerCart>();
    }
}