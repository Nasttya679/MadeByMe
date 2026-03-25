using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MadeByMe.Domain.Entities
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Title { get; set; }

        public string? Description { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price { get; set; }

        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        public string? SellerId { get; set; }

        public ApplicationUser? Seller { get; set; }

        [Column(TypeName = "numeric(3,2)")]
        public decimal Rating { get; set; } = 0.0m;

        [MaxLength(20)]
        public string Status { get; set; } = "active";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Comment> CommentsList { get; set; } = new List<Comment>();

        public ICollection<Photo> Photos { get; set; } = new List<Photo>();
    }
}