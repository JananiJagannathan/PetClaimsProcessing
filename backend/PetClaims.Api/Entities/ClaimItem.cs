using System.ComponentModel.DataAnnotations;
namespace PetClaims.Api.Entities
{
    public class ClaimItem
    {
        [Key]
public int ClaimItemId { get; set; }

        public int ClaimId { get; set; }

        public string Description { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public bool Eligible { get; set; }

        public Claim? Claim { get; set; }
    }
}