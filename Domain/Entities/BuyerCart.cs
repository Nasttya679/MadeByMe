using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MadeByMe.Domain.Entities
{
	public class BuyerCart
	{
        [Key]
        public int CartItemId { get; set; }

        // Видалити анотацію [ForeignKey] звідси
        public int CartId { get; set; }  // Залишаємо просто властивість

        [ForeignKey("CartId")]  // Або перенести сюди, якщо потрібно
        public Cart Cart { get; set; }

        [Required]
		public int PostId { get; set; }

		[ForeignKey("PostId")]
		public Post Post { get; set; }

		public int Quantity { get; set; } = 1;
	}
}
