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
    public class MessageService
    {
        private readonly DataDbContext _dbContext;

        public MessageService(DataDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Saves a new message to the database.
        /// </summary>
        /// <param name="senderId">ID of the sender.</param>
        /// <param name="chatId">ID of the chat where the message is sent.</param>
        /// <param name="content">Message content.</param>
        public async Task SaveMessageAsync(int senderId, Guid chatId, string content)
        {
            // Vérifiez si le chat existe
            var chat = await _dbContext.Chats.FirstOrDefaultAsync(c => c.UniqueId == chatId);
            if (chat == null)
            {
                throw new ArgumentException("Chat not found", nameof(chatId));
            }

            // Créez et enregistrez un nouveau message
            var message = new Message
            {
                UserId = senderId,
                Chat = chat,
                Content = content,
                CreatedAt = DateTime.Now
            };

            try
            {
                _dbContext.Messages.Add(message);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Gérez les exceptions ici (par exemple, journalisation)
                throw new InvalidOperationException("An error occurred while saving the message.", ex);
            }
        }

        /// <summary>
        /// Retrieves all messages for a specific chat.
        /// </summary>
        /// <param name="chatId">ID of the chat.</param>
        /// <returns>List of messages in the chat.</returns>
        public async Task<List<Message>> GetMessagesByChatAsync(Guid chatId)
        {
            List<Message> messages = await _dbContext.Messages
                .Where(m => m.Chat.UniqueId == chatId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
            return messages;
        }

        /// <summary>
        /// Retrieves all messages sent by a specific user.
        /// </summary>
        /// <param name="userId">ID of the user.</param>
        /// <returns>List of messages sent by the user.</returns>
        public async Task<List<Message>> GetMessagesByUserAsync(int userId)
        {
            return await _dbContext.Messages
                .Where(m => m.UserId == userId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }
    }

}
