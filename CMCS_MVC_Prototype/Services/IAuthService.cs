// Services/IAuthService.cs
using System.Text;
using CMCS_MVC_Prototype.Data;
using CMCS_MVC_Prototype.Models;

namespace CMCS_MVC_Prototype.Services
{
    public interface IAuthService
    {
        Task<User> Authenticate(string email, string password);
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
        Task<User> GetUserById(int id);
    }
}

