using MadeByMe.Domain.Entities;

namespace MadeByMe.Infrastructure.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        List<Category> GetAll();

        Category? GetById(int id);

        void Add(Category category);

        void Update(Category category);

        void Delete(Category category);
    }
}