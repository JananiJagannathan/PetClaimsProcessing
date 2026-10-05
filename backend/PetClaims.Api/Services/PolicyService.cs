using PetClaims.Api.Entities;
using PetClaims.Api.Repositories;

namespace PetClaims.Api.Services
{
    public class PolicyService : IPolicyService
    {
        private readonly IPolicyRepository _policyRepository;

        public PolicyService(IPolicyRepository policyRepository)
        {
            _policyRepository = policyRepository;
        }

        public async Task<Policy?> GetByPolicyNumberAsync(
            string policyNumber)
        {
            return await _policyRepository
                .GetByPolicyNumberAsync(policyNumber);
        }

        public async Task<List<Policy>> GetAllAsync()
        {
            return await _policyRepository.GetAllAsync();
        }
    }
}