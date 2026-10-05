namespace PetClaims.Api.DTOs
{
    public class MonthlyReportDto
    {
        public int Year { get; set; }

        public int Month { get; set; }

        public string MonthName { get; set; } = string.Empty;

        public int ClaimCount { get; set; }

        public decimal TotalClaimedAmount { get; set; }

        public decimal TotalApprovedAmount { get; set; }

        public decimal AverageClaimAmount { get; set; }
    }
}