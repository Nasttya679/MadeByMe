using System.ComponentModel.DataAnnotations;

namespace MadeByMe.Application.DTOs
{
	public class AssignPostToSellerDto
	{
		[Required]
		public string SellerId { get; set; }

		[Required]
		public int PostId { get; set; }
	}

	public class SellerPostsResponseDto
	{
		public string SellerId { get; set; }
		public string SellerName { get; set; }
		public List<PostResponseDto> Posts { get; set; }
	}
}