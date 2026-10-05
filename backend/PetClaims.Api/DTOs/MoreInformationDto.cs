using System.ComponentModel.DataAnnotations;

namespace PetClaims.Api.DTOs
{
    public class MoreInformationDto
    {
        [Required(ErrorMessage = "Reason is required.")]
        public string Reason { get; set; } = string.Empty;
    }
}