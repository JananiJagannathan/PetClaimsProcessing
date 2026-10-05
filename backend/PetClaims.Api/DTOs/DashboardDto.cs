namespace PetClaims.Api.DTOs
{
    public class DashboardDto
    {
        public int SubmittedCount { get; set; }

        public int UnderReviewCount { get; set; }

        public int ApprovedCount { get; set; }

        public int PartiallyApprovedCount { get; set; }

        public int RejectedCount { get; set; }

        public int MoreInformationRequiredCount { get; set; }

        public decimal TotalClaimedAmount { get; set; }

        public decimal TotalApprovedAmount { get; set; }
    }
}