using Microsoft.EntityFrameworkCore;
using CMCS_MVC_Prototype.Data;
using CMCS_MVC_Prototype.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMCS_MVC_Prototype.Services
{
    public class ClaimService : IClaimService
    {
        private readonly ApplicationDbContext _context;

        public ClaimService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Claim>> GetAllClaimsAsync()
        {
            return await _context.Claims.OrderByDescending(c => c.Submitted).ToListAsync();
        }

        public async Task<List<Claim>> GetClaimsByStatusAsync(string status)
        {
            return await _context.Claims
                .Where(c => c.Status == status)
                .OrderByDescending(c => c.Submitted)
                .ToListAsync();
        }

        public async Task<List<Claim>> GetClaimsByMonthAsync(string month)
        {
            return await _context.Claims
                .Where(c => c.Month == month)
                .OrderByDescending(c => c.Submitted)
                .ToListAsync();
        }


        public async Task<List<Claim>> GetClaimsByLecturerIdAsync(string lecturerId)
        {
            return await _context.Claims
                .Where(c => c.LecturerId.Contains(lecturerId))
                .OrderByDescending(c => c.Submitted)
                .ToListAsync();
        }

        public async Task<Claim> GetClaimByIdAsync(int id)
        {
            return await _context.Claims.FindAsync(id);
        }

        
        public async Task<int> CreateClaimAsync(Claim claim)
        {
            try
            {
                Console.WriteLine("=== CLAIM SERVICE DEBUG ===");
                Console.WriteLine($"Creating claim for: {claim.LecturerName}");
                Console.WriteLine($"Hours: {claim.HoursWorked}, Rate: {claim.HourlyRate}, Total: {claim.Total}");

                // Make sure LecturerId is generated (though it should be set by controller)
                if (string.IsNullOrEmpty(claim.LecturerId))
                {
                    claim.GenerateLecturerId();
                    Console.WriteLine($"Generated LecturerId: {claim.LecturerId}");
                }

                _context.Claims.Add(claim);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Claim created successfully with ID: {claim.Id}");
                return claim.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in ClaimService: {ex.Message}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                throw new Exception($"Error creating claim: {ex.Message}", ex);
            }
        }

        public async Task UpdateClaimStatusAsync(int id, string status, string actionBy)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim != null)
            {
                claim.Status = status;
                claim.ActionBy = actionBy;
                claim.ActionDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteClaimAsync(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim != null && claim.Status == "Pending")
            {
                _context.Claims.Remove(claim);
                await _context.SaveChangesAsync();
            }
            else if (claim != null && claim.Status != "Pending")
            {
                throw new InvalidOperationException("Only pending claims can be deleted.");
            }
        }
    }
    }
