using System.ComponentModel.DataAnnotations;

namespace PetClaims.Api.Entities
{
    public class Policy
    {
       [Key]
        public int PolicyId { get; set; }

        public string PolicyNumber { get; set; } = string.Empty;

        public string PetName { get; set; } = string.Empty;
        public string PetType { get; set; } = string.Empty;

public string Breed { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string Coverage { get; set; } = string.Empty;

        public decimal AnnualLimit { get; set; }

        public decimal Deductible { get; set; }

        public decimal ReimbursementPercentage { get; set; }

        public string Status { get; set; } = string.Empty;

        public ICollection<Claim> Claims { get; set; }
            = new List<Claim>();
    }
}