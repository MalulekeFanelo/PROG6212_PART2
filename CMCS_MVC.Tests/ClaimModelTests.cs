using CMCS_MVC_Prototype.Models;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace CMCS_MVC.Tests
{
    public class ClaimModelTests
    {
        [Theory]
        [InlineData("", false)] // Empty name
        [InlineData("   ", false)] // Whitespace name
        [InlineData("John", true)] // Valid name
        [InlineData("John Doe", true)] // Valid full name
        public void LecturerName_Validation(string lecturerName, bool expectedIsValid)
        {
            // Arrange
            var claim = new Claim
            {
                LecturerName = lecturerName,
                Month = "2025-03",
                HoursWorked = 10.0m,
                HourlyRate = 50.0m
            };

            var context = new ValidationContext(claim);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(claim, context, results, true);

            // Assert
            isValid.Should().Be(expectedIsValid);
        }

        [Theory]
        [InlineData(0, false)] // Zero hours
        [InlineData(-5, false)] // Negative hours
        [InlineData(0.1, true)] // Minimum valid hours
        [InlineData(1000, true)] // Maximum valid hours
        [InlineData(1001, false)] // Exceeds maximum
        public void HoursWorked_Validation(decimal hours, bool expectedIsValid)
        {
            // Arrange
            var claim = new Claim
            {
                LecturerName = "John Doe",
                Month = "2025-03",
                HoursWorked = hours,
                HourlyRate = 50.0m
            };

            var context = new ValidationContext(claim);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(claim, context, results, true);

            // Assert
            isValid.Should().Be(expectedIsValid);
        }

        [Theory]
        [InlineData(0, false)] // Zero rate
        [InlineData(-10, false)] // Negative rate
        [InlineData(1, true)] // Minimum valid rate
        [InlineData(500, true)] // Maximum valid rate
        [InlineData(501, false)] // Exceeds maximum
        public void HourlyRate_Validation(decimal rate, bool expectedIsValid)
        {
            // Arrange
            var claim = new Claim
            {
                LecturerName = "John Doe",
                Month = "2025-03",
                HoursWorked = 10.0m,
                HourlyRate = rate
            };

            var context = new ValidationContext(claim);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(claim, context, results, true);

            // Assert
            isValid.Should().Be(expectedIsValid);
        }

        [Fact]
        public void Month_Format_Validation()
        {
            // Arrange
            var claim = new Claim
            {
                LecturerName = "John Doe",
                Month = "2025-03", // Valid format
                HoursWorked = 10.0m,
                HourlyRate = 50.0m
            };

            var context = new ValidationContext(claim);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(claim, context, results, true);

            // Assert
            isValid.Should().BeTrue();
        }

        [Fact]
        public void Total_Calculation_IsAutomatic()
        {
            // Arrange
            var claim = new Claim
            {
                HoursWorked = 25.5m,
                HourlyRate = 80.0m
            };

            // Act & Assert
            claim.Total.Should().Be(2040.0m); // 25.5 * 80
        }

        [Fact]
        public void Status_Default_IsPending()
        {
            // Arrange & Act
            var claim = new Claim();

            // Assert
            claim.Status.Should().Be("Pending");
        }

        [Fact]
        public void Submitted_Default_IsRecent()
        {
            // Arrange & Act
            var claim = new Claim();

            // Assert
            claim.Submitted.Should().BeCloseTo(System.DateTime.Now, System.TimeSpan.FromSeconds(5));
        }
    }
}