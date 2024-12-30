namespace WhatsUpService.Core.Models
{
    public class Friend
    {
        public int Id { get; set; }

        public int UserId { get; set; } // One side of the friendship
        public int FriendUserId { get; set; } // The other side of the friendship

        public User User { get; set; } // Navigation property to the first user
        public User FriendUser { get; set; } // Navigation property to the second user
    }
}
