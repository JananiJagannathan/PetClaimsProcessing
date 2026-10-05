using Microsoft.EntityFrameworkCore;
using PetClaims.Api.Data;
using PetClaims.Api.Entities;

namespace PetClaims.Api.Repositories
{
    public class PolicyRepository : IPolicyRepository
    {
        private readonly AppDbContext _context;

        public PolicyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Policy?> GetByPolicyNumberAsync(string policyNumber)
        {
            return await _context.Policies
                .FirstOrDefaultAsync(p => p.PolicyNumber == policyNumber);
        }

        public async Task<List<Policy>> GetAllAsync()
        {
            return await _context.Policies
                .ToListAsync();
        }
    }
}