using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WhatsUpService.Core.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<Friend> Friends { get; set; } = new List<Friend>(); // List of friendships
        public ICollection<Message> Messages { get; set; } = new List<Message>(); // Messages sent by this user
    }
}

