using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MadeByMe.Domain.Entities
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        public string? UserId { get; set; }

        // [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required]
        public int PostId { get; set; }

        // [ForeignKey(nameof(PostId))]
        public Post? Post { get; set; }

        [Required]
        public string? Content { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}