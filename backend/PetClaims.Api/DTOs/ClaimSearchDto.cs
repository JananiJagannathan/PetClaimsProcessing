namespace PetClaims.Api.DTOs
{
    public class ClaimSearchDto
    {
        public string? Status { get; set; }

        public DateTime? FromDate { get; set; }

        public DateTime? ToDate { get; set; }

        public decimal? MinAmount { get; set; }

        public decimal? MaxAmount { get; set; }

        public string? PolicyNumber { get; set; }

        public string? Clinic { get; set; }
    }
}