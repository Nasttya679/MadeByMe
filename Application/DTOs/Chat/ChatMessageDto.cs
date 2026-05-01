using Microsoft.AspNetCore.Http;

namespace MadeByMe.Application.DTOs.Chat
{
    public class ChatMessageDto
    {
        public int ChatId { get; set; }

        public string? Content { get; set; }

        public IFormFile? Attachment { get; set; }
    }
}