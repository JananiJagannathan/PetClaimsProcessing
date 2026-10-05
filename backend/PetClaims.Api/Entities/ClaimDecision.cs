using System.ComponentModel.DataAnnotations;

namespace PetClaims.Api.Entities
{
    public class ClaimDecision
    {
        [Key]
        public int DecisionId { get; set; }

        public int ClaimId { get; set; }

        public string Decision { get; set; } = string.Empty;

        public DateTime DecisionDate { get; set; }

        public string Reason { get; set; } = string.Empty;

        public decimal ApprovedAmount { get; set; }

        public Claim? Claim { get; set; }
    }
}