using System.ComponentModel.DataAnnotations;

namespace MadeByMe.Domain.Entities
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        // Navigation properties
        public Product Product { get; set; }
    }
} 