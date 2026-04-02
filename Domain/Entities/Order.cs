using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MadeByMe.Domain.Entities
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? BuyerId { get; set; }

        [ForeignKey("BuyerId")]
        public ApplicationUser? Buyer { get; set; }

        [Required]
        [MaxLength(50)]
        public string? FirstName { get; set; }

        [MaxLength(50)]
        public string? LastName { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [Phone]
        public string? PhoneNumber { get; set; }

        [Required]
        [MaxLength(100)]
        public string? City { get; set; }

        [Required]
        [MaxLength(100)]
        public string? PostOffice { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalAmount { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Статуси: Pending, Paid, Shipped, Delivered, Cancelled

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}