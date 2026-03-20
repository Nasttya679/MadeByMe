namespace MadeByMe.Models
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}