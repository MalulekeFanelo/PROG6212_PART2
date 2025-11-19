// Services/AuthService.cs
using Microsoft.EntityFrameworkCore;
using CMCS_MVC_Prototype.Data;
using CMCS_MVC_Prototype.Models;
using System.Security.Cryptography;
using System.Text;

namespace CMCS_MVC_Prototype.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> Authenticate(string email, string password)
        {
            Console.WriteLine($"=== AUTH DEBUG ===");
            Console.WriteLine($"Email: {email}");
            Console.WriteLine($"Password provided: {password}");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

            if (user == null)
            {
                Console.WriteLine($"User not found with email: {email}");
                return null;
            }

            Console.WriteLine($"User found: {user.FullName}");
            Console.WriteLine($"Stored hash: {user.PasswordHash}");

            var inputHash = HashPassword(password);
            Console.WriteLine($"Input hash: {inputHash}");

            bool passwordMatches = VerifyPassword(password, user.PasswordHash);
            Console.WriteLine($"Password matches: {passwordMatches}");

            if (!passwordMatches)
                return null;

            return user;
        }
        public string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            return HashPassword(password) == passwordHash;
        }

        public async Task<User> GetUserById(int id)
        {
            return await _context.Users.FindAsync(id);
        }
    }
}
