using System.ComponentModel.DataAnnotations;

namespace PetClaims.Api.Entities
{
    public class ClaimDocument
    {
        [Key]
        public int DocumentId { get; set; }

        public int ClaimId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string DocumentType { get; set; } = string.Empty;

        public DateTime UploadedDate { get; set; }

        public Claim? Claim { get; set; }
    }
}