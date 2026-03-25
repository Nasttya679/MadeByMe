using System.ComponentModel.DataAnnotations;

namespace MadeByMe.Application.DTOs
{
    public class CreateCommentDto
    {
        [Required]
        public int PostId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string? Content { get; set; }
    }

    public class CommentResponseDto
    {
        public int CommentId { get; set; }

        public string? Content { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? AuthorName { get; set; }

        public string? AuthorAvatar { get; set; }
    }
}