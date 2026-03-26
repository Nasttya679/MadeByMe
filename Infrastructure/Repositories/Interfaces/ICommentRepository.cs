using MadeByMe.Domain.Entities;

namespace MadeByMe.Infrastructure.Repositories.Interfaces
{
    public interface ICommentRepository
    {
        Comment? GetById(int id);

        List<Comment> GetByPostId(int postId);

        void Add(Comment comment);

        void Delete(Comment comment);
    }
}