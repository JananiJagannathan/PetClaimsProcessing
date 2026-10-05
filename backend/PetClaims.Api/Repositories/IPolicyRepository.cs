using PetClaims.Api.Entities;

namespace PetClaims.Api.Repositories
{
    public interface IPolicyRepository
    {
        Task<Policy?> GetByPolicyNumberAsync(string policyNumber);

        Task<List<Policy>> GetAllAsync();
    }
}
