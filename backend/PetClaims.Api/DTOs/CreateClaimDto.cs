using System.ComponentModel.DataAnnotations;

namespace PetClaims.Api.DTOs
{
    public class CreateClaimDto
    {
        [Required]
        public string PolicyNumber { get; set; } = string.Empty;

        [Required]
        public DateTime TreatmentDate { get; set; }

        [Required]
        public string Clinic { get; set; } = string.Empty;

        [Required]
        public string Diagnosis { get; set; } = string.Empty;

        [Required]
        public string TreatmentDescription { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Claimed amount must be greater than zero.")]
        public decimal ClaimedAmount { get; set; }

        [Required]
        public string InvoiceNumber { get; set; } = string.Empty;

        public string? DocumentName { get; set; }
    }
}