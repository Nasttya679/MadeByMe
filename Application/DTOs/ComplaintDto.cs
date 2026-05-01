using System.ComponentModel.DataAnnotations;

namespace MadeByMe.Application.DTOs
{
    public class ComplaintDto
    {
        public int? PostId { get; set; }

        public string? SellerId { get; set; }

        [Required(ErrorMessage = "Оберіть причину скарги")]
        public string? Reason { get; set; }

        public string? Description { get; set; }
    }
}