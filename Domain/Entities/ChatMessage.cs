using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MadeByMe.Domain.Entities
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ChatId { get; set; }

        [ForeignKey("ChatId")]
        public Chat? Chat { get; set; }

        [Required]
        public string? SenderId { get; set; }

        [ForeignKey("SenderId")]
        public ApplicationUser? Sender { get; set; }

        public string? Content { get; set; }

        public string? FilePath { get; set; }

        public string? FileName { get; set; }

        public string? FileType { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeletedBySender { get; set; } = false;

        public bool IsDeletedByRecipient { get; set; } = false;

        public bool IsDeletedForEveryone { get; set; } = false;
    }
}