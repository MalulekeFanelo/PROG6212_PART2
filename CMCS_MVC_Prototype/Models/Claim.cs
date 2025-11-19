using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMCS_MVC_Prototype.Models
{
    public class Claim
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Lecturer Name is required")]
        [StringLength(100)]
        [Display(Name = "Lecturer Name")]
        public string LecturerName { get; set; } = string.Empty;


        [StringLength(20)]
        [Display(Name = "Lecturer ID")]
        public string LecturerId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Month is required")]
        [StringLength(7)]
        [Display(Name = "Month")]
        public string Month { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hours Worked is required")]
        [Range(0.1, 1000, ErrorMessage = "Hours must be between 0.1 and 1000")]
        [Display(Name = "Hours Worked")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal HoursWorked { get; set; }

        // REMOVED validation attributes since rate is set by HR
        [Display(Name = "Hourly Rate")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal HourlyRate { get; set; }

        [Display(Name = "Total Amount")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        [StringLength(500)]
        [Display(Name = "Additional Notes")]
        public string Notes { get; set; } = string.Empty;

        [StringLength(255)]
        [Display(Name = "Document Path")]
        public string DocumentPath { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending";

        [Display(Name = "Submitted Date")]
        public DateTime Submitted { get; set; } = DateTime.Now;

        [StringLength(100)]
        [Display(Name = "Approved/Rejected By")]
        public string ActionBy { get; set; } = string.Empty;

        [Display(Name = "Action Date")]
        public DateTime? ActionDate { get; set; }

        public void GenerateLecturerId()
        {
            if (string.IsNullOrEmpty(LecturerName))
                return;

            var random = new Random();
            var nameParts = LecturerName.Trim().Split(' ');
            var firstName = nameParts[0].ToLower();
            var lastName = nameParts.Length > 1 ? nameParts[^1].ToLower() : firstName;

            var lastPart = lastName.Length >= 3 ? lastName.Substring(0, 3) : lastName.PadRight(3, 'x');
            var firstPart = firstName.Length >= 2 ? firstName.Substring(0, 2) : firstName.PadRight(2, 'x');
            var randomDigits = random.Next(100, 999);

            LecturerId = $"{lastPart}{firstPart}{randomDigits}".ToUpper();
        }
    }
}