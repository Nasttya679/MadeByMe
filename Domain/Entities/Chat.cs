using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MadeByMe.Domain.Entities
{
    public class Chat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? BuyerId { get; set; }

        [ForeignKey("BuyerId")]
        public ApplicationUser? Buyer { get; set; }

        [Required]
        public string? SellerId { get; set; }

        [ForeignKey("SellerId")]
        public ApplicationUser? Seller { get; set; }

        public bool IsPinnedByBuyer { get; set; } = false;

        public bool IsPinnedBySeller { get; set; } = false;

        public bool IsDeletedByBuyer { get; set; } = false;

        public bool IsDeletedBySeller { get; set; } = false;

        public bool IsDeletedForEveryone { get; set; } = false;

        [Column("last_message_at")]
        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}