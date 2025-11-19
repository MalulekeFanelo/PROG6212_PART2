using CMCS_MVC_Prototype.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMCS_MVC_Prototype.Services
{
    public interface IClaimService
    {
        Task<List<Claim>> GetAllClaimsAsync();
        Task<List<Claim>> GetClaimsByStatusAsync(string status);
        Task<List<Claim>> GetClaimsByLecturerIdAsync(string lecturerId);
        Task<Claim> GetClaimByIdAsync(int id);
        Task<int> CreateClaimAsync(Claim claim);
        Task UpdateClaimStatusAsync(int id, string status, string actionBy);
        Task DeleteClaimAsync(int id);
        Task<List<Claim>> GetClaimsByMonthAsync(string month);

    }
}