using PetClaims.Api.DTOs;

namespace PetClaims.Api.Services
{
    public interface IClaimService
    {
        Task<ClaimDetailsDto> CreateClaimAsync(
            CreateClaimDto dto);

        Task<List<ClaimDetailsDto>> SearchClaimsAsync(
            ClaimSearchDto search);

        Task<ClaimDetailsDto?> GetClaimAsync(
            string claimNumber);

        Task<ClaimDetailsDto> UpdateClaimAsync(
            string claimNumber,
            UpdateClaimDto dto);

        Task<ClaimDetailsDto> MoveToReviewAsync(
            string claimNumber);

        Task<ClaimDetailsDto> ApproveClaimAsync(
            string claimNumber,
            ApproveClaimDto dto);

        Task<ClaimDetailsDto> RejectClaimAsync(
            string claimNumber,
            RejectClaimDto dto);
        Task<ClaimDetailsDto> RequestMoreInformationAsync(
    string claimNumber,
    MoreInformationDto dto);

        Task<DashboardDto> GetDashboardAsync();

        Task<List<MonthlyReportDto>> GetMonthlyReportAsync();
        Task<List<ClaimDetailsDto>> GetHighestValueClaimsAsync(
    int count);

Task<List<ClaimPolicyReportDto>> GetClaimPolicyReportAsync();
    }
}