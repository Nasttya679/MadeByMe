using System.ComponentModel.DataAnnotations;

namespace MadeByMe.Domain.Entities
{
    public class Photo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? FileName { get; set; }

        [Required]
        public string? FilePath { get; set; }

        [Required]
        public string? ContentType { get; set; }

        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public int? PostId { get; set; }

        public Post? Post { get; set; }
    }
}