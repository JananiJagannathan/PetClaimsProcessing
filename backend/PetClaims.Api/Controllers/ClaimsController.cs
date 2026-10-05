using Microsoft.AspNetCore.Mvc;
using PetClaims.Api.DTOs;
using PetClaims.Api.Services;

namespace PetClaims.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClaimsController : ControllerBase
    {
        private readonly IClaimService _claimService;

        public ClaimsController(IClaimService claimService)
        {
            _claimService = claimService;
        }

        // POST: /api/claims
        [HttpPost]
        public async Task<ActionResult<ClaimDetailsDto>> CreateClaim(
            [FromBody] CreateClaimDto dto)
        {
            var claim = await _claimService.CreateClaimAsync(dto);

            return CreatedAtAction(
                nameof(GetClaim),
                new { claimNumber = claim.ClaimNumber },
                claim);
        }

        // GET: /api/claims
        [HttpGet]
        public async Task<ActionResult<List<ClaimDetailsDto>>> SearchClaims(
            [FromQuery] ClaimSearchDto search)
        {
            var claims =
                await _claimService.SearchClaimsAsync(search);

            return Ok(claims);
        }

        // GET: /api/claims/CLM-xxxx
        [HttpGet("{claimNumber}")]
        public async Task<ActionResult<ClaimDetailsDto>> GetClaim(
            string claimNumber)
        {
            var claim =
                await _claimService.GetClaimAsync(claimNumber);

            if (claim == null)
            {
                return NotFound(new
                {
                    message = "Claim not found."
                });
            }

            return Ok(claim);
        }
        [HttpPut("{claimNumber}")]
public async Task<ActionResult<ClaimDetailsDto>> UpdateClaim(
    string claimNumber,
    [FromBody] UpdateClaimDto dto)
{
    var claim =
        await _claimService.UpdateClaimAsync(
            claimNumber,
            dto);

    return Ok(claim);
}

        // POST: /api/claims/CLM-xxxx/review
        [HttpPost("{claimNumber}/review")]
        public async Task<ActionResult<ClaimDetailsDto>> MoveToReview(
            string claimNumber)
        {
            var claim =
                await _claimService.MoveToReviewAsync(claimNumber);

            return Ok(claim);
        }

        // POST: /api/claims/CLM-xxxx/approve
        [HttpPost("{claimNumber}/approve")]
        public async Task<ActionResult<ClaimDetailsDto>> ApproveClaim(
            string claimNumber,
            [FromBody] ApproveClaimDto dto)
        {
            var claim =
                await _claimService.ApproveClaimAsync(
                    claimNumber,
                    dto);

            return Ok(claim);
        }

        // POST: /api/claims/CLM-xxxx/reject
        [HttpPost("{claimNumber}/reject")]
        public async Task<ActionResult<ClaimDetailsDto>> RejectClaim(
            string claimNumber,
            [FromBody] RejectClaimDto dto)
        {
            var claim =
                await _claimService.RejectClaimAsync(
                    claimNumber,
                    dto);

            return Ok(claim);
        }

        // POST: /api/claims/{claimNumber}/more-information
[HttpPost("{claimNumber}/more-information")]
public async Task<ActionResult<ClaimDetailsDto>>
    RequestMoreInformation(
        string claimNumber,
        [FromBody] MoreInformationDto dto)
{
    var claim =
        await _claimService.RequestMoreInformationAsync(
            claimNumber,
            dto);

    return Ok(claim);
}

        [HttpGet("dashboard")]
public async Task<ActionResult<DashboardDto>> GetDashboard()
{
    var dashboard =
        await _claimService.GetDashboardAsync();

    return Ok(dashboard);
}

[HttpGet("reports/monthly")]
public async Task<ActionResult<List<MonthlyReportDto>>>
    GetMonthlyReport()
{
    var report =
        await _claimService.GetMonthlyReportAsync();

    return Ok(report);
}
    }
}