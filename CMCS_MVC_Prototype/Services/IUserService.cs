using System.Collections.Generic;
using System.Threading.Tasks;
using CMCS_MVC_Prototype.Data;
using CMCS_MVC_Prototype.Models;

namespace CMCS_MVC_Prototype.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(int id);
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByLecturerIdAsync(string lecturerId);
        Task<int> CreateUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task DeactivateUserAsync(int id);
        Task ActivateUserAsync(int id);
    }
}

