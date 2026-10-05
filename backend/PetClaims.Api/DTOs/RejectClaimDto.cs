using System.ComponentModel.DataAnnotations;

namespace PetClaims.Api.DTOs
{
    public class RejectClaimDto
    {
        [Required(ErrorMessage = "Rejection reason is required.")]
        public string Reason { get; set; } = string.Empty;
    }
}