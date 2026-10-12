using PetClaims.Api.DTOs;
using PetClaims.Api.Entities;
using PetClaims.Api.Exceptions;
using PetClaims.Api.Repositories;

namespace PetClaims.Api.Services
{
    public class ClaimService : IClaimService
    {
        private readonly IClaimRepository _claimRepository;
        private readonly IPolicyRepository _policyRepository;
        private readonly ILogger<ClaimService> _logger;

        public ClaimService(
            IClaimRepository claimRepository,
            IPolicyRepository policyRepository,
            ILogger<ClaimService> logger)
        {
            _claimRepository = claimRepository;
            _policyRepository = policyRepository;
            _logger = logger;
        }

        // ==================================================
        // 1. CREATE / SUBMIT CLAIM
        // ==================================================

        public async Task<ClaimDetailsDto> CreateClaimAsync(
            CreateClaimDto dto)
        {
            var policy =
                await _policyRepository
                    .GetByPolicyNumberAsync(dto.PolicyNumber);

            if (policy == null)
            {
                throw new NotFoundException(
                    "Policy not found.");
            }

            if (!policy.Status.Equals(
                    "Active",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessException(
                    "Policy is not active.");
            }

            if (dto.TreatmentDate.Date < policy.StartDate.Date ||
                dto.TreatmentDate.Date > policy.EndDate.Date)
            {
                throw new BusinessException(
                    "Treatment date must be within the policy period.");
            }

            if (dto.ClaimedAmount <= 0)
            {
                throw new BusinessException(
                    "Claimed amount must be greater than zero.");
            }

            var duplicateInvoice =
                await _claimRepository.InvoiceExistsAsync(
                    policy.PolicyId,
                    dto.InvoiceNumber);

            if (duplicateInvoice)
            {
                throw new DuplicateInvoiceException(
                    "Duplicate invoice number for this policy.");
            }

            var claim = new Claim
            {
                ClaimNumber = GenerateClaimNumber(),

                PolicyId = policy.PolicyId,

                TreatmentDate = dto.TreatmentDate,

                Clinic = dto.Clinic,

                Diagnosis = dto.Diagnosis,

                TreatmentDescription =
                    dto.TreatmentDescription,

                ClaimedAmount = dto.ClaimedAmount,

                ApprovedAmount = 0,

                InvoiceNumber = dto.InvoiceNumber,

                Status = "Submitted",

                CreatedDate = DateTime.UtcNow
            };

            if (!string.IsNullOrWhiteSpace(dto.DocumentName))
            {
                claim.ClaimDocuments.Add(
                    new ClaimDocument
                    {
                        FileName = dto.DocumentName,
                        DocumentType = "Invoice",
                        UploadedDate = DateTime.UtcNow
                    });
            }

            await _claimRepository.AddAsync(claim);

            await _claimRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Claim {ClaimNumber} submitted successfully for policy {PolicyNumber} with claimed amount {ClaimedAmount}",
                claim.ClaimNumber,
                policy.PolicyNumber,
                claim.ClaimedAmount);

            return await MapToDetailsDtoAsync(
                claim,
                policy);
        }

        // ==================================================
        // 2. GET CLAIM DETAILS
        // ==================================================

        public async Task<ClaimDetailsDto?> GetClaimAsync(
            string claimNumber)
        {
            var claim =
                await _claimRepository
                    .GetByClaimNumberAsync(claimNumber);

            if (claim == null ||
                claim.Policy == null)
            {
                return null;
            }

            return await MapToDetailsDtoAsync(
                claim,
                claim.Policy);
        }

        // ==================================================
        // 3. UPDATE EDITABLE CLAIM
        // ==================================================

        public async Task<ClaimDetailsDto> UpdateClaimAsync(
            string claimNumber,
            UpdateClaimDto dto)
        {
            var claim =
                await GetRequiredClaimAsync(claimNumber);

            if (claim.Status != "Submitted")
            {
                throw new BusinessException(
                    "Only submitted claims can be updated.");
            }

            var policy = claim.Policy!;

            if (dto.ClaimedAmount <= 0)
            {
                throw new BusinessException(
                    "Claimed amount must be greater than zero.");
            }

            if (dto.TreatmentDate.Date < policy.StartDate.Date ||
                dto.TreatmentDate.Date > policy.EndDate.Date)
            {
                throw new BusinessException(
                    "Treatment date must be within the policy period.");
            }

            var duplicateInvoice =
                await _claimRepository.InvoiceExistsAsync(
                    policy.PolicyId,
                    dto.InvoiceNumber,
                    claim.ClaimId);

            if (duplicateInvoice)
            {
                throw new DuplicateInvoiceException(
                    "Duplicate invoice number for this policy.");
            }

            claim.TreatmentDate =
                dto.TreatmentDate;

            claim.Clinic =
                dto.Clinic;

            claim.Diagnosis =
                dto.Diagnosis;

            claim.TreatmentDescription =
                dto.TreatmentDescription;

            claim.ClaimedAmount =
                dto.ClaimedAmount;

            claim.InvoiceNumber =
                dto.InvoiceNumber;

            await _claimRepository.UpdateAsync(claim);

            await _claimRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Claim {ClaimNumber} updated successfully",
                claim.ClaimNumber);

            return await MapToDetailsDtoAsync(
                claim,
                policy);
        }

        // ==================================================
        // 4. SEARCH / FILTER CLAIMS
        // ==================================================

        public async Task<List<ClaimDetailsDto>>
            SearchClaimsAsync(
                ClaimSearchDto search)
        {
            var claims =
                await _claimRepository
                    .SearchAsync(search);

            var result =
                new List<ClaimDetailsDto>();

            foreach (var claim in claims)
            {
                if (claim.Policy != null)
                {
                    result.Add(
                        await MapToDetailsDtoAsync(
                            claim,
                            claim.Policy));
                }
            }

            return result;
        }

        // ==================================================
        // 5. MOVE TO REVIEW
        // ==================================================

        public async Task<ClaimDetailsDto> MoveToReviewAsync(
            string claimNumber)
        {
            var claim =
                await GetRequiredClaimAsync(claimNumber);

            if (claim.Status != "Submitted" &&
                claim.Status != "More Information Required")
            {
                throw new BusinessException(
                    "Only submitted or more information required claims can be moved to review.");
            }

            claim.Status = "Under Review";

            claim.ClaimDecisions.Add(
                new ClaimDecision
                {
                    Decision = "Under Review",

                    DecisionDate =
                        DateTime.UtcNow,

                    Reason =
                        "Claim moved to review.",

                    ApprovedAmount = 0
                });

            await _claimRepository.UpdateAsync(claim);
            await _claimRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Claim {ClaimNumber} moved to Under Review",
                claim.ClaimNumber);

            return await MapToDetailsDtoAsync(
                claim,
                claim.Policy!);
        }

        // ==================================================
        // 6. APPROVE / PARTIALLY APPROVE
        // ==================================================

        public async Task<ClaimDetailsDto> ApproveClaimAsync(
            string claimNumber,
            ApproveClaimDto dto)
        {
            var claim =
                await GetRequiredClaimAsync(claimNumber);

            if (IsFinalised(claim.Status))
            {
                throw new BusinessException(
                    "A finalised claim cannot be processed again.");
            }

            if (claim.Status != "Under Review")
            {
                throw new BusinessException(
                    "Claim must be under review before approval.");
            }

            var policy = claim.Policy!;

            // Claimed Amount - Deductible
            var eligibleAmount =
                Math.Max(
                    0,
                    claim.ClaimedAmount -
                    policy.Deductible);

            // Eligible Amount * Reimbursement %
            var calculatedAmount =
                eligibleAmount *
                policy.ReimbursementPercentage / 100;

            var alreadyApproved =
                await _claimRepository
                    .GetTotalApprovedForPolicyYearAsync(
                        policy.PolicyId,
                        claim.TreatmentDate.Year);

            var remainingAnnualLimit =
                Math.Max(
                    0,
                    policy.AnnualLimit -
                    alreadyApproved);

            var maximumAllowed =
                Math.Min(
                    calculatedAmount,
                    claim.ClaimedAmount);

            maximumAllowed =
                Math.Min(
                    maximumAllowed,
                    remainingAnnualLimit);

            decimal approvedAmount;

            if (dto.RequestedApprovedAmount.HasValue)
            {
                approvedAmount =
                    dto.RequestedApprovedAmount.Value;

                if (approvedAmount > maximumAllowed)
                {
                    throw new BusinessException(
                        "Approved amount exceeds the allowed amount.");
                }
            }
            else
            {
                approvedAmount =
                    maximumAllowed;
            }

            if (approvedAmount < 0)
            {
                throw new BusinessException(
                    "Approved amount cannot be negative.");
            }

            claim.ApprovedAmount =
                approvedAmount;

            if (approvedAmount < maximumAllowed)
            {
                claim.Status =
                    "Partially Approved";
            }
            else
            {
                claim.Status =
                    "Approved";
            }

            claim.ClaimDecisions.Add(
                new ClaimDecision
                {
                    Decision =
                        claim.Status,

                    DecisionDate =
                        DateTime.UtcNow,

                    Reason =
                        dto.Reason,

                    ApprovedAmount =
                        approvedAmount
                });

            await _claimRepository.UpdateAsync(claim);
            await _claimRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Claim {ClaimNumber} processed with status {Status} and approved amount {ApprovedAmount}",
                claim.ClaimNumber,
                claim.Status,
                claim.ApprovedAmount);

            return await MapToDetailsDtoAsync(
                claim,
                policy);
        }

        // ==================================================
        // 7. REJECT CLAIM
        // ==================================================

        public async Task<ClaimDetailsDto> RejectClaimAsync(
            string claimNumber,
            RejectClaimDto dto)
        {
            var claim =
                await GetRequiredClaimAsync(claimNumber);

            if (IsFinalised(claim.Status))
            {
                throw new BusinessException(
                    "A finalised claim cannot be processed again.");
            }

            if (claim.Status != "Under Review")
            {
                throw new BusinessException(
                    "Claim must be under review before rejection.");
            }

            if (string.IsNullOrWhiteSpace(dto.Reason))
            {
                throw new BusinessException(
                    "Rejection reason is required.");
            }

            claim.Status =
                "Rejected";

            claim.ApprovedAmount =
                0;

            claim.ClaimDecisions.Add(
                new ClaimDecision
                {
                    Decision =
                        "Rejected",

                    DecisionDate =
                        DateTime.UtcNow,

                    Reason =
                        dto.Reason,

                    ApprovedAmount =
                        0
                });

            await _claimRepository.UpdateAsync(claim);
            await _claimRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Claim {ClaimNumber} rejected. Reason: {Reason}",
                claim.ClaimNumber,
                dto.Reason);

            return await MapToDetailsDtoAsync(
                claim,
                claim.Policy!);
        }

        // ==================================================
        // 8. REQUEST MORE INFORMATION
        // ==================================================

        public async Task<ClaimDetailsDto>
            RequestMoreInformationAsync(
                string claimNumber,
                MoreInformationDto dto)
        {
            var claim =
                await GetRequiredClaimAsync(claimNumber);

            if (IsFinalised(claim.Status))
            {
                throw new BusinessException(
                    "A finalised claim cannot be processed again.");
            }

            if (claim.Status != "Under Review")
            {
                throw new BusinessException(
                    "Claim must be under review before requesting more information.");
            }

            if (string.IsNullOrWhiteSpace(dto.Reason))
            {
                throw new BusinessException(
                    "Reason is required when requesting more information.");
            }

            claim.Status =
                "More Information Required";

            claim.ClaimDecisions.Add(
                new ClaimDecision
                {
                    Decision =
                        "More Information Required",

                    DecisionDate =
                        DateTime.UtcNow,

                    Reason =
                        dto.Reason,

                    ApprovedAmount =
                        0
                });

            await _claimRepository.UpdateAsync(claim);
            await _claimRepository.SaveChangesAsync();

            _logger.LogInformation(
                "More information requested for claim {ClaimNumber}. Reason: {Reason}",
                claim.ClaimNumber,
                dto.Reason);

            return await MapToDetailsDtoAsync(
                claim,
                claim.Policy!);
        }

        // ==================================================
        // 9. DASHBOARD
        // ==================================================

        public async Task<DashboardDto> GetDashboardAsync()
        {
            return await _claimRepository
                .GetDashboardAsync();
        }

        // ==================================================
        // 10. MONTHLY REPORT
        // ==================================================

        public async Task<List<MonthlyReportDto>>
            GetMonthlyReportAsync()
        {
            return await _claimRepository
                .GetMonthlyReportAsync();
        }

        // ==================================================
        // 11. HIGHEST VALUE CLAIMS
        // LINQ: OrderByDescending + Take
        // ==================================================

        public async Task<List<ClaimDetailsDto>>
            GetHighestValueClaimsAsync(
                int count)
        {
            if (count <= 0)
            {
                throw new BusinessException(
                    "Count must be greater than zero.");
            }

            var claims =
                await _claimRepository
                    .GetHighestValueClaimsAsync(count);

            var result =
                new List<ClaimDetailsDto>();

            foreach (var claim in claims)
            {
                if (claim.Policy != null)
                {
                    result.Add(
                        await MapToDetailsDtoAsync(
                            claim,
                            claim.Policy));
                }
            }

            return result;
        }

        // ==================================================
        // 12. CLAIM + POLICY REPORT
        // LINQ: Join
        // ==================================================

        public async Task<List<ClaimPolicyReportDto>>
            GetClaimPolicyReportAsync()
        {
            return await _claimRepository
                .GetClaimPolicyReportAsync();
        }

        // ==================================================
        // PRIVATE HELPERS
        // ==================================================

        private async Task<Claim> GetRequiredClaimAsync(
            string claimNumber)
        {
            var claim =                await _claimRepository
                    .GetByClaimNumberAsync(claimNumber);

            if (claim == null)
            {
                throw new NotFoundException(
                    "Claim not found.");
            }

            if (claim.Policy == null)
            {
                throw new NotFoundException(
                    "Policy information not found.");
            }

            return claim;
        }

        // --------------------------------------------------
        // CHECK IF CLAIM IS FINALISED
        // --------------------------------------------------

        private static bool IsFinalised(
            string status)
        {
            return status == "Approved" ||
                   status == "Partially Approved" ||
                   status == "Rejected";
        }

        // --------------------------------------------------
        // GENERATE CLAIM NUMBER
        // --------------------------------------------------

        private static string GenerateClaimNumber()
        {
            return
                $"CLM-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        }

        // --------------------------------------------------
        // ENTITY -> DTO
        // --------------------------------------------------

        private async Task<ClaimDetailsDto>
            MapToDetailsDtoAsync(
                Claim claim,
                Policy policy)
        {
            var eligibleAmount =
                Math.Max(
                    0,
                    claim.ClaimedAmount -
                    policy.Deductible);

            var totalApproved =
                await _claimRepository
                    .GetTotalApprovedForPolicyYearAsync(
                        policy.PolicyId,
                        claim.TreatmentDate.Year);

            var remainingAnnualLimit =
                Math.Max(
                    0,
                    policy.AnnualLimit -
                    totalApproved);

            return new ClaimDetailsDto
            {
                ClaimNumber =
                    claim.ClaimNumber,

                PolicyNumber =
                    policy.PolicyNumber,

                PetName =
                    policy.PetName,
                PetType = policy.PetType,
                Breed = policy.Breed,

                TreatmentDate =
                    claim.TreatmentDate,

                Clinic =
                    claim.Clinic,

                Diagnosis =
                    claim.Diagnosis,

                TreatmentDescription =
                    claim.TreatmentDescription,

                ClaimedAmount =
                    claim.ClaimedAmount,

                ApprovedAmount =
                    claim.ApprovedAmount,

                InvoiceNumber =
                    claim.InvoiceNumber,

                Status =
                    claim.Status,

                CreatedDate =
                    claim.CreatedDate,

                PolicyStartDate =
                    policy.StartDate,

                PolicyEndDate =
                    policy.EndDate,

                Coverage =
                    policy.Coverage,

                AnnualLimit =
                    policy.AnnualLimit,

                Deductible =
                    policy.Deductible,

                ReimbursementPercentage =
                    policy.ReimbursementPercentage,

                EligibleAmount =
                    eligibleAmount,

                RemainingAnnualLimit =
                    remainingAnnualLimit,

                DecisionHistory =
                    claim.ClaimDecisions
                        .OrderBy(d => d.DecisionDate)
                        .Select(
                            d => new ClaimDecisionDto
                            {
                                Decision =
                                    d.Decision,

                                DecisionDate =
                                    d.DecisionDate,

                                Reason =
                                    d.Reason,

                                ApprovedAmount =
                                    d.ApprovedAmount
                            })
                        .ToList()
            };
        }
    }
}
  