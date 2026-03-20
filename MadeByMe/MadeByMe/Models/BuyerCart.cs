namespace MadeByMe.Models
{
    public class BuyerCart
    {
        public int CartId { get; set; }
        public Cart? Cart { get; set; }

        public int PostId { get; set; }
        public Post? Post { get; set; }

        public int Quantity { get; set; }
    }
}