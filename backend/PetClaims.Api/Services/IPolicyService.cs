using PetClaims.Api.Entities;

namespace PetClaims.Api.Services
{
    public interface IPolicyService
    {
        Task<Policy?> GetByPolicyNumberAsync(string policyNumber);

        Task<List<Policy>> GetAllAsync();
    }
}