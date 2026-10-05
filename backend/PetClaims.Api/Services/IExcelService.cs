namespace PetClaims.Api.Services
{
    public interface IExcelService
    {
        Task<int> ImportClaimsAsync(
            Stream fileStream);

        Task<byte[]> ExportClaimsAsync();
    }
}