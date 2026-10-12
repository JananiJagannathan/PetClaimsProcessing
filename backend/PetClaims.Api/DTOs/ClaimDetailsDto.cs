namespace PetClaims.Api.DTOs
{
    public class ClaimDetailsDto
    {
        public string ClaimNumber { get; set; } = string.Empty;

        public string PolicyNumber { get; set; } = string.Empty;

        public string PetName { get; set; } = string.Empty;

        public string PetType { get; set; } = string.Empty;

        public string Breed { get; set; } = string.Empty;

        public DateTime TreatmentDate { get; set; }

        public string Clinic { get; set; } = string.Empty;

        public string Diagnosis { get; set; } = string.Empty;

        public string TreatmentDescription { get; set; } = string.Empty;

        public decimal ClaimedAmount { get; set; }

        public decimal ApprovedAmount { get; set; }

        public string InvoiceNumber { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }

        public DateTime PolicyStartDate { get; set; }

        public DateTime PolicyEndDate { get; set; }

        public string Coverage { get; set; } = string.Empty;

        public decimal AnnualLimit { get; set; }

        public decimal Deductible { get; set; }

        public decimal ReimbursementPercentage { get; set; }

        public decimal EligibleAmount { get; set; }

        public decimal RemainingAnnualLimit { get; set; }

        public List<ClaimDecisionDto> DecisionHistory { get; set; }
            = new List<ClaimDecisionDto>();
    }

    public class ClaimDecisionDto
    {
        public string Decision { get; set; } = string.Empty;

        public DateTime DecisionDate { get; set; }

        public string Reason { get; set; } = string.Empty;

        public decimal ApprovedAmount { get; set; }
    }
}