using System.ComponentModel.DataAnnotations;

namespace MadeByMe.Application.DTOs
{
    public class CreateCategoryDto
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        //public string? Description { get; set; }
    }

    public class UpdateCategoryDto
    {
        [Required]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        //public string? Description { get; set; }
    }

    public class CategoryResponseDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        //public string? Description { get; set; }
        public int PostsCount { get; set; }
    }
}