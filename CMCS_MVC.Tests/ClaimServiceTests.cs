using Microsoft.EntityFrameworkCore;
using CMCS_MVC_Prototype.Data;
using CMCS_MVC_Prototype.Models;
using CMCS_MVC_Prototype.Services;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMCS_MVC.Tests
{
    public class ClaimServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ClaimService _claimService;

        public ClaimServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _claimService = new ClaimService(_context);
        }

        [Fact]
        public async Task CreateClaimAsync_ValidClaim_SavesToDatabase()
        {
            // Arrange
            var claim = new Claim
            {
                LecturerName = "Test User",
                LecturerId = "TEST123",
                Month = "2025-03",
                HoursWorked = 10.0m,
                HourlyRate = 50.0m,
                Status = "Pending"
            };

            // Act
            var claimId = await _claimService.CreateClaimAsync(claim);

            // Assert
            claimId.Should().BeGreaterThan(0);
            var savedClaim = await _context.Claims.FindAsync(claimId);
            savedClaim.Should().NotBeNull();
            savedClaim.LecturerName.Should().Be("Test User");
        }

        [Fact]
        public async Task GetAllClaimsAsync_ReturnsAllClaims()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim { LecturerName = "User1", LecturerId = "USER001", Month = "2025-03", HoursWorked = 10, HourlyRate = 50 },
                new Claim { LecturerName = "User2", LecturerId = "USER002", Month = "2025-03", HoursWorked = 20, HourlyRate = 60 }
            };

            await _context.Claims.AddRangeAsync(claims);
            await _context.SaveChangesAsync();

            // Act
            var result = await _claimService.GetAllClaimsAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(c => c.LecturerName == "User1");
            result.Should().Contain(c => c.LecturerName == "User2");
        }

        [Fact]
        public async Task GetClaimsByStatusAsync_ReturnsFilteredClaims()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim { LecturerName = "User1", LecturerId = "USER001", Month = "2025-03", HoursWorked = 10, HourlyRate = 50, Status = "Pending" },
                new Claim { LecturerName = "User2", LecturerId = "USER002", Month = "2025-03", HoursWorked = 20, HourlyRate = 60, Status = "Approved" },
                new Claim { LecturerName = "User3", LecturerId = "USER003", Month = "2025-03", HoursWorked = 15, HourlyRate = 70, Status = "Pending" }
            };

            await _context.Claims.AddRangeAsync(claims);
            await _context.SaveChangesAsync();

            // Act
            var pendingClaims = await _claimService.GetClaimsByStatusAsync("Pending");

            // Assert
            pendingClaims.Should().HaveCount(2);
            pendingClaims.Should().OnlyContain(c => c.Status == "Pending");
        }

        [Fact]
        public async Task UpdateClaimStatusAsync_ValidId_UpdatesStatus()
        {
            // Arrange
            var claim = new Claim
            {
                LecturerName = "Test User",
                LecturerId = "TEST123",
                Month = "2025-03",
                HoursWorked = 10.0m,
                HourlyRate = 50.0m,
                Status = "Pending"
            };

            await _context.Claims.AddAsync(claim);
            await _context.SaveChangesAsync();

            // Act
            await _claimService.UpdateClaimStatusAsync(claim.Id, "Approved", "Coordinator");

            // Assert
            var updatedClaim = await _context.Claims.FindAsync(claim.Id);
            updatedClaim.Status.Should().Be("Approved");
            updatedClaim.ActionBy.Should().Be("Coordinator");
            updatedClaim.ActionDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task DeleteClaimAsync_PendingClaim_RemovesFromDatabase()
        {
            // Arrange
            var claim = new Claim
            {
                LecturerName = "Test User",
                LecturerId = "TEST123",
                Month = "2025-03",
                HoursWorked = 10.0m,
                HourlyRate = 50.0m,
                Status = "Pending"
            };

            await _context.Claims.AddAsync(claim);
            await _context.SaveChangesAsync();

            // Act
            await _claimService.DeleteClaimAsync(claim.Id);

            // Assert
            var deletedClaim = await _context.Claims.FindAsync(claim.Id);
            deletedClaim.Should().BeNull();
        }

        [Fact]
        public async Task DeleteClaimAsync_NonPendingClaim_ThrowsException()
        {
            // Arrange
            var claim = new Claim
            {
                LecturerName = "Test User",
                LecturerId = "TEST123",
                Month = "2025-03",
                HoursWorked = 10.0m,
                HourlyRate = 50.0m,
                Status = "Approved" // Not pending
            };

            await _context.Claims.AddAsync(claim);
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _claimService.DeleteClaimAsync(claim.Id));
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}