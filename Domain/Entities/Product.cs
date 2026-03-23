using System.ComponentModel.DataAnnotations;

namespace MadeByMe.Domain.Entities
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        public string ImageUrl { get; set; }

        public string SellerId { get; set; }
        public ApplicationUser Seller { get; set; }
    }
} 