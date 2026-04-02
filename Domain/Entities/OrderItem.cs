using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MadeByMe.Domain.Entities
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        [Required]
        public int PostId { get; set; }

        [ForeignKey("PostId")]
        public Post? Post { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal PriceAtPurchase { get; set; }
    }
}