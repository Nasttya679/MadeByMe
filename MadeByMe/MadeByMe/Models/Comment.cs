using MadeByMe.Models;

namespace MadeByMe.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Text { get; set; } = null!;

        public int PostId { get; set; }
        public Post? Post { get; set; }

        public string UserId { get; set; } = null!;
        public ApplicationUser? User { get; set; }
    }
}