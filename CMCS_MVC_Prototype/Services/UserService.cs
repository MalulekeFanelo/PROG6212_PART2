// Services/UserService.cs
using Microsoft.EntityFrameworkCore;
using CMCS_MVC_Prototype.Data;
using CMCS_MVC_Prototype.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMCS_MVC_Prototype.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .OrderBy(u => u.Role)
                .ThenBy(u => u.LastName)
                .ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetUserByLecturerIdAsync(string lecturerId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.LecturerId == lecturerId);
        }

        public async Task<int> CreateUserAsync(User user)
        {
            // Check if email already exists
            var existingUser = await GetUserByEmailAsync(user.Email);
            if (existingUser != null)
            {
                throw new Exception("A user with this email already exists.");
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user.Id;
        }

        public async Task UpdateUserAsync(User user)
        {
            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser != null)
            {
                _context.Entry(existingUser).CurrentValues.SetValues(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeactivateUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }


        public async Task ActivateUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.IsActive = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
