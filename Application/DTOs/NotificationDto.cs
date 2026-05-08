using System;

namespace MadeByMe.Application.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }

        public string Message { get; set; } = string.Empty;

        public string? ActionUrl { get; set; }

        public string? SenderAvatar { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsRead { get; set; }
    }
}