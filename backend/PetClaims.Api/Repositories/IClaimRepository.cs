using PetClaims.Api.DTOs;
using PetClaims.Api.Entities;

namespace PetClaims.Api.Repositories
{
    public interface IClaimRepository
    {
        Task AddAsync(Claim claim);

        Task UpdateAsync(Claim claim);

        Task<Claim?> GetByClaimNumberAsync(
            string claimNumber);

        Task<List<Claim>> SearchAsync(
            ClaimSearchDto search);

        Task<bool> InvoiceExistsAsync(
            int policyId,
            string invoiceNumber,
            int? excludeClaimId = null);

        Task<decimal> GetTotalApprovedForPolicyYearAsync(
            int policyId,
            int year);

        Task<List<Claim>> GetAllAsync();

        Task<DashboardDto> GetDashboardAsync();

        Task<List<MonthlyReportDto>> GetMonthlyReportAsync();
        Task<List<Claim>> GetHighestValueClaimsAsync(int count);

        Task<List<ClaimPolicyReportDto>> GetClaimPolicyReportAsync();

        Task SaveChangesAsync();
    }
}
