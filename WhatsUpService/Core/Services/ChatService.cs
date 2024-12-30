using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhatsUpService.Core.Data;
using WhatsUpService.Core.Models;

namespace WhatsUpService.Core.Services
{
    public class ChatService
    {
        private readonly DataDbContext _dataDbContext;

        public ChatService(DataDbContext dataDbContext)
        {
            _dataDbContext = dataDbContext;
        }

        public async Task<Chat> GetChatById(int id)
        {
            return await _dataDbContext.Chats.FindAsync(id);
        }

        public async Task CreateChatAsync(int senderId, int receiverId)
        {
            // Check if users are friends
            var areFriends = await _dataDbContext.Friends.AnyAsync(f =>
                (f.UserId == senderId && f.FriendUserId == receiverId) ||
                (f.UserId == receiverId && f.FriendUserId == senderId));

            if (!areFriends)
            {
                throw new InvalidOperationException("Users must be friends to chat.");
            }
            var senderUser = await _dataDbContext.Users.FindAsync(senderId);
            var receiverUser = await _dataDbContext.Users.FindAsync(receiverId);
            Guid guid = Guid.NewGuid();

            // Create chat
            var chat = new Chat
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                UniqueId = guid,
                Sender = senderUser,
                Receiver = receiverUser
            };

            var chat2 = new Chat
            {
                SenderId = receiverId,
                ReceiverId = senderId,
                UniqueId = guid,
                Sender = receiverUser,
                Receiver = senderUser
            };

            await _dataDbContext.Chats.AddAsync(chat);
            await _dataDbContext.Chats.AddAsync(chat2);
            await _dataDbContext.SaveChangesAsync();
        }

        public async Task<List<Chat>> GetChatsByUserId(int userId)
        {
            return await _dataDbContext.Chats
                .Where(c => c.SenderId == userId)
                .ToListAsync();
        }



    }
}
