namespace PetClaims.Api.DTOs
{
    public class ClaimPolicyReportDto
    {
        public string ClaimNumber { get; set; } = string.Empty;

        public string PolicyNumber { get; set; } = string.Empty;

        public string PetName { get; set; } = string.Empty;

        public string Clinic { get; set; } = string.Empty;

        public DateTime TreatmentDate { get; set; }

        public decimal ClaimedAmount { get; set; }

        public decimal ApprovedAmount { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
