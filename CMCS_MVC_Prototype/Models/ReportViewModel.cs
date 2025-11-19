// Models/ReportViewModel.cs
namespace CMCS_MVC_Prototype.Models
{
    public class ReportViewModel
    {
        public string Month { get; set; } = string.Empty;
        public int TotalClaims { get; set; }
        public int PendingClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public decimal TotalAmount { get; set; }
    }
}