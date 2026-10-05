using Microsoft.EntityFrameworkCore;
using PetClaims.Api.Data;
using PetClaims.Api.DTOs;
using PetClaims.Api.Entities;

namespace PetClaims.Api.Repositories
{
    public class ClaimRepository : IClaimRepository
    {
        private readonly AppDbContext _context;

        public ClaimRepository(AppDbContext context)
        {
            _context = context;
        }

        // --------------------------------------------------
        // ADD CLAIM
        // --------------------------------------------------

        public async Task AddAsync(Claim claim)
        {
            await _context.Claims.AddAsync(claim);
        }

        // --------------------------------------------------
        // UPDATE CLAIM
        // --------------------------------------------------

        public Task UpdateAsync(Claim claim)
        {
            _context.Claims.Update(claim);

            return Task.CompletedTask;
        }

        // --------------------------------------------------
        // GET CLAIM BY CLAIM NUMBER
        // --------------------------------------------------

        public async Task<Claim?> GetByClaimNumberAsync(
            string claimNumber)
        {
            return await _context.Claims
                .Include(c => c.Policy)
                .Include(c => c.ClaimDecisions)
                .Include(c => c.ClaimItems)
                .Include(c => c.ClaimDocuments)
                .FirstOrDefaultAsync(
                    c => c.ClaimNumber == claimNumber);
        }

        // --------------------------------------------------
        // SEARCH / FILTER CLAIMS
        // --------------------------------------------------

        public async Task<List<Claim>> SearchAsync(
            ClaimSearchDto search)
        {
            var query = _context.Claims
                .Include(c => c.Policy)
                .AsQueryable();

            // Filter by status
            if (!string.IsNullOrWhiteSpace(search.Status))
            {
                query = query.Where(
                    c => c.Status == search.Status);
            }

            // Filter by start date
            if (search.FromDate.HasValue)
            {
                query = query.Where(
                    c => c.TreatmentDate >= search.FromDate.Value);
            }

            // Filter by end date
            if (search.ToDate.HasValue)
            {
                query = query.Where(
                    c => c.TreatmentDate <= search.ToDate.Value);
            }

            // Filter by minimum amount
            if (search.MinAmount.HasValue)
            {
                query = query.Where(
                    c => c.ClaimedAmount >= search.MinAmount.Value);
            }

            // Filter by maximum amount
            if (search.MaxAmount.HasValue)
            {
                query = query.Where(
                    c => c.ClaimedAmount <= search.MaxAmount.Value);
            }

            // Filter by policy number
            if (!string.IsNullOrWhiteSpace(search.PolicyNumber))
            {
                query = query.Where(
                    c => c.Policy != null &&
                         c.Policy.PolicyNumber ==
                         search.PolicyNumber);
            }

            // Filter by clinic
            if (!string.IsNullOrWhiteSpace(search.Clinic))
            {
                query = query.Where(
                    c => c.Clinic.Contains(search.Clinic));
            }

            return await query
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        // --------------------------------------------------
        // DUPLICATE INVOICE CHECK
        // --------------------------------------------------

        public async Task<bool> InvoiceExistsAsync(
            int policyId,
            string invoiceNumber,
            int? excludeClaimId = null)
        {
            return await _context.Claims.AnyAsync(c =>
                c.PolicyId == policyId &&
                c.InvoiceNumber == invoiceNumber &&
                (!excludeClaimId.HasValue ||
                 c.ClaimId != excludeClaimId.Value));
        }

        // --------------------------------------------------
        // TOTAL APPROVED AMOUNT FOR POLICY / YEAR
        // --------------------------------------------------

        public async Task<decimal>
            GetTotalApprovedForPolicyYearAsync(
                int policyId,
                int year)
        {
            return await _context.Claims
                .Where(c =>
                    c.PolicyId == policyId &&
                    c.TreatmentDate.Year == year)
                .SumAsync(c => c.ApprovedAmount);
        }

        // --------------------------------------------------
        // GET ALL CLAIMS
        // --------------------------------------------------

        public async Task<List<Claim>> GetAllAsync()
        {
            return await _context.Claims
                .Include(c => c.Policy)
                .Include(c => c.ClaimDecisions)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }

        // --------------------------------------------------
        // DASHBOARD
        // --------------------------------------------------

        public async Task<DashboardDto> GetDashboardAsync()
        {
            return new DashboardDto
            {
                SubmittedCount =
                    await _context.Claims.CountAsync(
                        c => c.Status == "Submitted"),

                UnderReviewCount =
                    await _context.Claims.CountAsync(
                        c => c.Status == "Under Review"),

                ApprovedCount =
                    await _context.Claims.CountAsync(
                        c => c.Status == "Approved"),

                PartiallyApprovedCount =
                    await _context.Claims.CountAsync(
                        c => c.Status == "Partially Approved"),

                RejectedCount =
                    await _context.Claims.CountAsync(
                        c => c.Status == "Rejected"),

                MoreInformationRequiredCount =
                    await _context.Claims.CountAsync(
                        c => c.Status ==
                             "More Information Required"),

                TotalClaimedAmount =
                    await _context.Claims.SumAsync(
                        c => c.ClaimedAmount),

                TotalApprovedAmount =
                    await _context.Claims.SumAsync(
                        c => c.ApprovedAmount)
            };
        }

        // --------------------------------------------------
        // MONTHLY CLAIM REPORT
        // --------------------------------------------------

        public async Task<List<MonthlyReportDto>>
            GetMonthlyReportAsync()
        {
            var report = await _context.Claims
                .GroupBy(c => new
                {
                    c.TreatmentDate.Year,
                    c.TreatmentDate.Month
                })
                .Select(group => new MonthlyReportDto
                {
                    Year = group.Key.Year,

                    Month = group.Key.Month,

                    ClaimCount = group.Count(),

                    TotalClaimedAmount =
                        group.Sum(c => c.ClaimedAmount),

                    TotalApprovedAmount =
                        group.Sum(c => c.ApprovedAmount),

                    AverageClaimAmount =
                        group.Average(c => c.ClaimedAmount)
                })
                .OrderByDescending(r => r.Year)
                .ThenByDescending(r => r.Month)
                .ToListAsync();

            // Convert month number into month name
            // Example: 9 -> September
            foreach (var item in report)
            {
                item.MonthName =
                    new DateTime(
                        item.Year,
                        item.Month,
                        1)
                    .ToString("MMMM");
            }

            return report;
        }
        // --------------------------------------------------
// HIGHEST VALUE CLAIMS
// OrderByDescending + Take
// --------------------------------------------------

public async Task<List<Claim>> GetHighestValueClaimsAsync(
    int count)
{
    return await _context.Claims
        .Include(c => c.Policy)
        .OrderByDescending(c => c.ClaimedAmount)
        .Take(count)
        .ToListAsync();
}

// --------------------------------------------------
// CLAIM + POLICY REPORT
// LINQ JOIN
// --------------------------------------------------

public async Task<List<ClaimPolicyReportDto>>
    GetClaimPolicyReportAsync()
{
    return await _context.Claims
        .Join(
            _context.Policies,

            claim => claim.PolicyId,

            policy => policy.PolicyId,

            (claim, policy) =>
                new ClaimPolicyReportDto
                {
                    ClaimNumber =
                        claim.ClaimNumber,

                    PolicyNumber =
                        policy.PolicyNumber,

                    PetName =
                        policy.PetName,

                    Clinic =
                        claim.Clinic,

                    TreatmentDate =
                        claim.TreatmentDate,

                    ClaimedAmount =
                        claim.ClaimedAmount,

                    ApprovedAmount =
                        claim.ApprovedAmount,

                    Status =
                        claim.Status
                })
        .OrderByDescending(
            x => x.ClaimedAmount)
        .ToListAsync();
}
        // --------------------------------------------------
        // SAVE CHANGES
        // --------------------------------------------------

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}