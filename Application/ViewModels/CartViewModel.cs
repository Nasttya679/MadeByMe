using MadeByMe.Domain.Entities;

namespace MadeByMe.Application.ViewModels
{
    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public decimal TotalPrice { get; set; }

        public int ItemCount => Items?.Count ?? 0;
    }
}
