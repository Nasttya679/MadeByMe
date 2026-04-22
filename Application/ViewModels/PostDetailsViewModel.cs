using MadeByMe.Domain.Entities;

namespace MadeByMe.Application.ViewModels
{
    public class PostDetailsViewModel
    {
        public Post? Post { get; set; }

        public List<Comment>? CommentsList { get; set; }

        public bool IsFavorite { get; set; } = false;
    }
}
