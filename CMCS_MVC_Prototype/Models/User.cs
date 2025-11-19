// Models/User.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMCS_MVC_Prototype.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        // REMOVE [Required] from PasswordHash - it's generated from the password field
        [StringLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal HourlyRate { get; set; }

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "Lecturer"; // HR, Lecturer, Coordinator, Manager

        [StringLength(20)]
        public string LecturerId { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation property
        public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();

        public string FullName => $"{FirstName} {LastName}";

        public void GenerateLecturerId()
        {
            var random = new Random();
            var lastPart = LastName.Length >= 3 ? LastName.Substring(0, 3) : LastName.PadRight(3, 'x');
            var firstPart = FirstName.Length >= 2 ? FirstName.Substring(0, 2) : FirstName.PadRight(2, 'x');
            var randomDigits = random.Next(100, 999);

            LecturerId = $"{lastPart}{firstPart}{randomDigits}".ToUpper();
        }
    }
}