using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MadeByMe.Domain.Entities
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        public string? ActionUrl { get; set; }

        public string? SenderAvatar { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsSent { get; set; } = false;

        public bool IsRead { get; set; } = false;
    }
}