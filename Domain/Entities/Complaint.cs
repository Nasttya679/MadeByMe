using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MadeByMe.Domain.Entities
{
    public class Complaint
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? ReporterId { get; set; }

        [ForeignKey("ReporterId")]
        public ApplicationUser? Reporter { get; set; }

        public int? PostId { get; set; }

        [ForeignKey("PostId")]
        public Post? Post { get; set; }

        public string? SellerId { get; set; }

        [ForeignKey("SellerId")]
        public ApplicationUser? Seller { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Reason { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}