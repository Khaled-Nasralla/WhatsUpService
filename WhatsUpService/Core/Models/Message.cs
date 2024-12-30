using System;


namespace WhatsUpService.Core.Models
{
    public class Message
    {
        public int Id { get; set; } // Unique identifier for the message
        public string Content { get; set; } // The text content of the message
        public DateTime CreatedAt { get; set; } // Timestamp of when the message was created

        // References to the sender and associated chat
        public int UserId { get; set; } // Foreign key to User
        public User User { get; set; } // Navigation property to the sender user

        public int ChatId { get; set; } // Foreign key to Chat
        public Chat Chat { get; set; } // Navigation property to the chat
    }
}