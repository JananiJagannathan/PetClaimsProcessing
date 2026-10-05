using System.ComponentModel.DataAnnotations;

namespace PetClaims.Api.DTOs
{
    public class ApproveClaimDto
    {
        [Range(0, double.MaxValue)]
        public decimal? RequestedApprovedAmount { get; set; }

        public string Reason { get; set; } = string.Empty;
    }
}