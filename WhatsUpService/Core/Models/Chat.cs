namespace WhatsUpService.Core.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public Guid UniqueId { get; set; } = Guid.NewGuid(); // Unique identifier for the chat
        public int SenderId { get; set; } // Foreign key for the sender
        public int ReceiverId { get; set; } // Foreign key for the receiver

        public User Sender { get; set; } // Navigation property for the sender
        public User Receiver { get; set; } // Navigation property for the receiver

        public ICollection<Message> Messages { get; set; } = new List<Message>(); // Messages associated with this chat
    }
}
