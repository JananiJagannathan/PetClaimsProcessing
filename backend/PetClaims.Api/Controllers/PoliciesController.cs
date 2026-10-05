using Microsoft.AspNetCore.Mvc;
using PetClaims.Api.Entities;
using PetClaims.Api.Services;

namespace PetClaims.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PoliciesController : ControllerBase
    {
        private readonly IPolicyService _policyService;

        public PoliciesController(
            IPolicyService policyService)
        {
            _policyService = policyService;
        }

        // GET: /api/policies
        [HttpGet]
        public async Task<ActionResult<List<Policy>>> GetPolicies()
        {
            var policies =
                await _policyService.GetAllAsync();

            return Ok(policies);
        }

        // GET: /api/policies/POL001
        [HttpGet("{policyNumber}")]
        public async Task<ActionResult<Policy>> GetPolicy(
            string policyNumber)
        {
            var policy =
                await _policyService
                    .GetByPolicyNumberAsync(policyNumber);

            if (policy == null)
            {
                return NotFound(new
                {
                    message = "Policy not found."
                });
            }

            return Ok(policy);
        }
    }
}