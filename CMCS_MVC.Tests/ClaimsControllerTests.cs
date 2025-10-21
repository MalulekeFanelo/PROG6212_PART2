using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CMCS_MVC_Prototype.Controllers;
using CMCS_MVC_Prototype.Models;
using CMCS_MVC_Prototype.Data;
using CMCS_MVC_Prototype.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CMCS_MVC.Tests
{
    public class ClaimsControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ClaimService _claimService;
        private readonly ClaimsController _controller;

        public ClaimsControllerTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _claimService = new ClaimService(_context);
            _controller = new ClaimsController(_claimService);
        }

        [Fact]
        public void Create_GET_ReturnsViewResult()
        {
            // Act
            var result = _controller.Create();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Create_POST_WithValidModel_RedirectsToIndex()
        {
            // Arrange
            var claim = new Claim
            {
                LecturerName = "John Doe",
                Month = "2025-03",
                HoursWorked = 10.0m,
                HourlyRate = 50.0m,
                Notes = "Test claim"
            };
            IFormFile document = null; // No file uploaded

            // Act
            var result = await _controller.Create(claim, document);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>()
                .Which.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task Create_POST_WithValidModel_GeneratesLecturerId()
        {
            // Arrange
            var claim = new Claim
            {
                LecturerName = "John Doe",
                Month = "2025-03",
                HoursWorked = 10.0m,
                HourlyRate = 50.0m
            };
            IFormFile document = null;

            // Act
            await _controller.Create(claim, document);

            // Assert
            claim.LecturerId.Should().NotBeNullOrEmpty();
            claim.LecturerId.Should().MatchRegex("^[A-Z]{5}\\d{3}$"); // Format: ABCDE123
        }

        [Fact]
        public async Task Create_POST_WithValidModel_SavesToDatabase()
        {
            // Arrange
            var claim = new Claim
            {
                LecturerName = "Jane Smith",
                Month = "2025-03",
                HoursWorked = 20.0m,
                HourlyRate = 75.0m
            };
            IFormFile document = null;

            // Act
            await _controller.Create(claim, document);

            // Assert
            var savedClaim = await _context.Claims.FirstOrDefaultAsync(c => c.LecturerName == "Jane Smith");
            savedClaim.Should().NotBeNull();
            savedClaim.Total.Should().Be(1500.0m); // 20 * 75
            savedClaim.Status.Should().Be("Pending");
        }

        [Fact]
        public async Task Create_POST_WithInvalidModel_ReturnsView()
        {
            // Arrange
            var claim = new Claim
            {
                LecturerName = "", // Invalid - empty name
                Month = "2025-03",
                HoursWorked = 10.0m,
                HourlyRate = 50.0m
            };
            IFormFile document = null;
            _controller.ModelState.AddModelError("LecturerName", "Required");

            // Act
            var result = await _controller.Create(claim, document);

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Create_POST_WithFileUpload_SavesDocumentPath()
        {
            // Arrange
            var claim = new Claim
            {
                LecturerName = "John Doe",
                Month = "2025-03",
                HoursWorked = 10.0m,
                HourlyRate = 50.0m
            };

            var mockFile = new Mock<IFormFile>();
            var content = "Hello World from a Fake File";
            var fileName = "test.pdf";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            mockFile.Setup(_ => _.OpenReadStream()).Returns(ms);
            mockFile.Setup(_ => _.FileName).Returns(fileName);
            mockFile.Setup(_ => _.Length).Returns(ms.Length);

            // Act
            await _controller.Create(claim, mockFile.Object);

            // Assert
            var savedClaim = await _context.Claims.FirstOrDefaultAsync(c => c.LecturerName == "John Doe");
            savedClaim.Should().NotBeNull();
            savedClaim.DocumentPath.Should().NotBeNullOrEmpty();
            savedClaim.DocumentPath.Should().Contain("test.pdf");
        }

        [Fact]
        public async Task Create_POST_WithZeroHours_ReturnsViewWithError()
        {
            // Arrange
            var claim = new Claim
            {
                LecturerName = "John Doe",
                Month = "2025-03",
                HoursWorked = 0m, // Invalid - zero hours
                HourlyRate = 50.0m
            };
            IFormFile document = null;
            _controller.ModelState.AddModelError("HoursWorked", "Hours must be greater than 0");

            // Act
            var result = await _controller.Create(claim, document);

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Create_POST_WithNegativeRate_ReturnsViewWithError()
        {
            // Arrange
            var claim = new Claim
            {
                LecturerName = "John Doe",
                Month = "2025-03",
                HoursWorked = 10.0m,
                HourlyRate = -10.0m // Invalid - negative rate
            };
            IFormFile document = null;
            _controller.ModelState.AddModelError("HourlyRate", "Rate must be positive");

            // Act
            var result = await _controller.Create(claim, document);

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void LecturerId_Generation_FromDifferentNames_ProducesDifferentIds()
        {
            // Arrange
            var claim1 = new Claim { LecturerName = "John Smith" };
            var claim2 = new Claim { LecturerName = "Sarah Johnson" };

            // Act
            claim1.GenerateLecturerId();
            claim2.GenerateLecturerId();

            // Assert
            claim1.LecturerId.Should().NotBe(claim2.LecturerId);
        }

        [Fact]
        public void LecturerId_Generation_FromSameName_ProducesDifferentIds()
        {
            // Arrange
            var claim1 = new Claim { LecturerName = "John Smith" };
            var claim2 = new Claim { LecturerName = "John Smith" };

            // Act
            claim1.GenerateLecturerId();
            claim2.GenerateLecturerId();

            // Assert
            claim1.LecturerId.Should().NotBe(claim2.LecturerId);
        }

        [Fact]
        public void Total_Calculation_IsCorrect()
        {
            // Arrange
            var claim = new Claim
            {
                HoursWorked = 15.5m,
                HourlyRate = 100.0m
            };

            // Act & Assert
            claim.Total.Should().Be(1550.0m); // 15.5 * 100
        }

        [Fact]
        public async Task Create_POST_SetsCorrectTimestamp()
        {
            // Arrange
            var claim = new Claim
            {
                LecturerName = "John Doe",
                Month = "2025-03",
                HoursWorked = 10.0m,
                HourlyRate = 50.0m
            };
            IFormFile document = null;
            var beforeSubmit = DateTime.Now;

            // Act
            await _controller.Create(claim, document);

            // Assert
            var savedClaim = await _context.Claims.FirstOrDefaultAsync(c => c.LecturerName == "John Doe");
            savedClaim.Should().NotBeNull();
            savedClaim.Submitted.Should().BeCloseTo(beforeSubmit, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public async Task Create_POST_WithNullDocument_SetsEmptyDocumentPath()
        {
            // Arrange
            var claim = new Claim
            {
                LecturerName = "Test User",
                Month = "2025-03",
                HoursWorked = 10.0m,
                HourlyRate = 50.0m
            };
            IFormFile document = null;

            // Act
            await _controller.Create(claim, document);

            // Assert
            var savedClaim = await _context.Claims.FirstOrDefaultAsync(c => c.LecturerName == "Test User");
            savedClaim.Should().NotBeNull();
            savedClaim.DocumentPath.Should().BeEmpty();
        }

        [Fact]
        public async Task Create_POST_WithEmptyFile_SetsEmptyDocumentPath()
        {
            // Arrange
            var claim = new Claim
            {
                LecturerName = "Test User",
                Month = "2025-03",
                HoursWorked = 10.0m,
                HourlyRate = 50.0m
            };

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(_ => _.Length).Returns(0); // Empty file
            mockFile.Setup(_ => _.FileName).Returns("empty.txt");

            // Act
            await _controller.Create(claim, mockFile.Object);

            // Assert
            var savedClaim = await _context.Claims.FirstOrDefaultAsync(c => c.LecturerName == "Test User");
            savedClaim.Should().NotBeNull();
            savedClaim.DocumentPath.Should().BeEmpty();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}