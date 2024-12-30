using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Threading.Tasks;
using WhatsUpService.Core.Data;
using WhatsUpService.Core.Models;

namespace WhatsUpService.Core.Services
{
    public class UserService
    {
        private readonly DataDbContext _dataDbContext;
        private readonly HashingService _hashingService;

        public UserService(DataDbContext dataDbContext, HashingService hashingService)
        {
            _dataDbContext = dataDbContext ?? throw new ArgumentNullException(nameof(dataDbContext));
            _hashingService = hashingService ?? throw new ArgumentNullException(nameof(hashingService));
        }

        public async Task<User> GetUserById(int id)
        {


            return await _dataDbContext.Users.FindAsync(id);
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            var user = await _dataDbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user != null && _hashingService.VerifyHash(password, user.Password))
            {
                return $"OK:{user.Id}";
            }
            return "Invalid credentials.";
        }

        public async Task<string> SignUpAsync(string username, string password, string email)
        {
            if (await _dataDbContext.Users.AnyAsync(u => u.Username == username))
            {
                return "Username already exists.";
            }
            bool isPasswordValid = _hashingService.IsPasswordStrong(password);
            if (!isPasswordValid)
            {
                return "Password is not strong enough.";
            }

            var hashedPassword = _hashingService.Hash(password);

            _dataDbContext.Users.Add(new User { Username = username, Password = hashedPassword, Email = email });
            await _dataDbContext.SaveChangesAsync();

            return "User registered successfully";
        }

        public async Task<User> CreateUser(User user)
        {
            _dataDbContext.Users.Add(user);
            await _dataDbContext.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateUser(User user)
        {
            user.UpdatedAt = DateTime.Now;
            _dataDbContext.Users.Update(user);
            await _dataDbContext.SaveChangesAsync();
            return user;
        }

        public async Task<User> DeleteUser(int id)
        {
            var user = await _dataDbContext.Users.FindAsync(id);
            if (user == null) return null;

            _dataDbContext.Users.Remove(user);
            await _dataDbContext.SaveChangesAsync();
            return user;
        }

        public async Task<List<User>> GetUsers()
        {
            return await _dataDbContext.Users.ToListAsync();
        }

        public async Task<List<User>> SearchFriends(string name)

        {

            return await _dataDbContext.Users
                .Where(f => f.Username.Contains(name))
                .ToListAsync();
        }
    }
}
