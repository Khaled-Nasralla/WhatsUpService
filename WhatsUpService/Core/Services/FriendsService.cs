using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // Ajoutez cet espace de noms
using WhatsUpService.Core.Data;
using WhatsUpService.Core.Models;

namespace WhatsUpService.Core.Services
{
    public class FriendsService
    {
        private readonly DataDbContext _dataDbContext;

        public FriendsService(DataDbContext dataDbContext)
        {
            _dataDbContext = dataDbContext;
        }

        public async Task<Friend> AddFriend(int userId1, int userId2)
        {
            var friend = new Friend
            {
                UserId = userId1,
                FriendUserId = userId2,

            };

            var friend2 = new Friend
            {
                UserId = userId2,
                FriendUserId = userId1,
            };

            _dataDbContext.Friends.Add(friend);
            _dataDbContext.Friends.Add(friend2);
            await _dataDbContext.SaveChangesAsync();

            return friend;
        }

        public async Task<Friend> DeleteFriend(int id)
        {
            var friend = await _dataDbContext.Friends.FindAsync(id);
            if (friend == null) return null;

            _dataDbContext.Remove(friend);
            await _dataDbContext.SaveChangesAsync();

            return friend;
        }

        public async Task<List<Friend>> GetFriends()
        {
            return await _dataDbContext.Friends.ToListAsync();
        }

        public async Task<Friend> GetFriendsByUserId(int userId)
        {
            return await _dataDbContext.Friends.FirstOrDefaultAsync(f => f.UserId == userId);

        }

        public async Task<List<Friend>> SearchFriends(string name)
        {
            return await _dataDbContext.Friends
                .Where(f => f.User.Username.Contains(name) || f.FriendUser.Username.Contains(name))
                .ToListAsync();
        }

        //verfier si un utilisateur est ami avec un autre ou pas
        public async Task<bool> IsFriend(int userId1, int userId2)
        {
            return await _dataDbContext.Friends.AnyAsync(f => (f.UserId == userId1 && f.FriendUserId == userId2) || (f.UserId == userId2 && f.FriendUserId == userId1));
        }

    }
}
