
namespace MadeByMe.Application.ViewModels
{
    public class CartItemViewModel
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public decimal PricePerItem { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
    }
}
