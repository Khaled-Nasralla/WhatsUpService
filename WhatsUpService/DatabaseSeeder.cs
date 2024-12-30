using WhatsUpService.Core.Data;
using WhatsUpService.Core.Services;
using WhatsUpService.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WhatsUpService
{
    internal class DatabaseSeeder
    {
        internal static async Task SeedAsync(DataDbContext dbContext, HashingService hashingService, UserService userService)
        {
            if (!dbContext.Users.Any())
            {
                var users = new List<User>
                {
                    new User { Username = "Khaled", Password = hashingService.Hash("password"), Email = "khaled@admin.com", CreatedAt = DateTime.UtcNow },
                    new User { Username = "Gregoire", Password = hashingService.Hash("password"), Email = "greg@admin.com", CreatedAt = DateTime.UtcNow },
                    new User { Username = "Thomas", Password = hashingService.Hash("password"), Email = "thomas@admin.com", CreatedAt = DateTime.UtcNow }
                };

                foreach (var user in users)
                {
                    await userService.CreateUser(user);
                }
            }

            // Create friends before creating chats
            if (!dbContext.Friends.Any())
            {
                var Khaled = dbContext.Users.First(u => u.Username == "Khaled");
                var Gregoire = dbContext.Users.First(u => u.Username == "Gregoire");

                var friend = new Friend
                {
                    UserId = Khaled.Id,
                    FriendUserId = Gregoire.Id
                };
                var friend2 = new Friend
                {
                    UserId = Gregoire.Id,
                    FriendUserId = Khaled.Id
                };

                await dbContext.Friends.AddAsync(friend);
                await dbContext.Friends.AddAsync(friend2);
                await dbContext.SaveChangesAsync();
            }

            // Now you can create chats between friends
            if (!dbContext.Chats.Any())
            {
                var Khaled = dbContext.Users.First(u => u.Username == "Khaled");
                var Gregoire = dbContext.Users.First(u => u.Username == "Gregoire");

                Guid guid = Guid.NewGuid();
                var chat = new Chat
                {

                    SenderId = Khaled.Id,
                    ReceiverId = Gregoire.Id,
                    UniqueId =guid,
                    Sender = Khaled,
                    Receiver = Gregoire
                };
                var chat2 = new Chat
                {
                    SenderId = Gregoire.Id,
                    ReceiverId = Khaled.Id,
                    UniqueId = guid,
                    Sender = Gregoire,
                    Receiver = Khaled
                };
                await dbContext.Chats.AddAsync(chat);
                await dbContext.Chats.AddAsync(chat2);
                await dbContext.SaveChangesAsync();
            }

            if (!dbContext.Messages.Any())
            {
                var Khaled = dbContext.Users.First(u => u.Username == "Khaled");
                var Gregoire = dbContext.Users.First(u => u.Username == "Gregoire");
                var chat = dbContext.Chats.First(c => c.SenderId == Khaled.Id && c.ReceiverId == Gregoire.Id);

                var messages = new List<Message>
                {
                    new Message
                    {
                        UserId = Khaled.Id,
                        Chat = chat,
                        Content = "Hello Gregoire",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Message
                    {
                        UserId = Gregoire.Id,
                        Chat = chat,
                        Content = "Hello Khaled",
                        CreatedAt = DateTime.UtcNow
                    }
                };

                await dbContext.Messages.AddRangeAsync(messages);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
