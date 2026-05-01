namespace MadeByMe.Application.DTOs.Chat
{
    public class ChatSummaryDto
    {
        public int ChatId { get; set; }

        public string? InterlocutorName { get; set; }

        public string? InterlocutorId { get; set; }

        public string? InterlocutorPhoto { get; set; }

        public string? LastMessage { get; set; }

        public DateTime LastMessageTime { get; set; }

        public bool IsPinned { get; set; }

        public int UnreadCount { get; set; }
    }
}