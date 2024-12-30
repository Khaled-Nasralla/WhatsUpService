using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace WhatsUpService.Core.Services
{
    public class HashingService
    {
        public string Hash(string input)
        {
            using var sha256 = SHA256.Create();
            return string.Concat(sha256.ComputeHash(Encoding.UTF8.GetBytes(input)).Select(b => b.ToString("x2")));
        }

        public bool VerifyHash(string input, string hash) => Hash(input) == hash;


        public bool IsPasswordStrong(string password)
        {
            //&& password.Any(char.IsDigit) && password.Any(char.IsUpper) && password.Any(char.IsLower) && password.Any(char.IsPunctuation);
            return password.Length >= 8;

        }
    }

}
