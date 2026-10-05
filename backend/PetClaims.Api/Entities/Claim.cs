using System.ComponentModel.DataAnnotations;
namespace PetClaims.Api.Entities
{
    public class Claim
    {
        [Key]
public int ClaimId { get; set; }

        public string ClaimNumber { get; set; } = string.Empty;

        public int PolicyId { get; set; }

        public DateTime TreatmentDate { get; set; }

        public string Clinic { get; set; } = string.Empty;

        public string Diagnosis { get; set; } = string.Empty;

        public string TreatmentDescription { get; set; } = string.Empty;

        public decimal ClaimedAmount { get; set; }

        public decimal ApprovedAmount { get; set; }

        public string InvoiceNumber { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; }

        public Policy? Policy { get; set; }

        public ICollection<ClaimItem> ClaimItems { get; set; }
            = new List<ClaimItem>();

        public ICollection<ClaimDecision> ClaimDecisions { get; set; }
            = new List<ClaimDecision>();

        public ICollection<ClaimDocument> ClaimDocuments { get; set; }
            = new List<ClaimDocument>();
    }
}