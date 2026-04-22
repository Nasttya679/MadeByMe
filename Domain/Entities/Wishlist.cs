using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MadeByMe.Domain.Entities
{
    public class Wishlist
    {
        [Key]
        public int Id { get; set; }

        [Required]
        required public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required]
        public int PostId { get; set; }

        [ForeignKey("PostId")]
        public Post? Post { get; set; }

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}