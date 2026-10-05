using Microsoft.EntityFrameworkCore;
using PetClaims.Api.Entities;

namespace PetClaims.Api.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // Do not insert policies again if data already exists
            if (await context.Policies.AnyAsync())
            {
                return;
            }

            var policies = new List<Policy>
            {
                new Policy
                {
                    PolicyNumber = "POL001",
                    PetName = "Bruno",
                    StartDate = new DateTime(2026, 1, 1),
                    EndDate = new DateTime(2026, 12, 31),
                    Coverage = "Accident and Illness",
                    AnnualLimit = 50000,
                    Deductible = 1000,
                    ReimbursementPercentage = 80,
                    Status = "Active"
                },

                new Policy
                {
                    PolicyNumber = "POL002",
                    PetName = "Milo",
                    StartDate = new DateTime(2025, 1, 1),
                    EndDate = new DateTime(2025, 12, 31),
                    Coverage = "Accident and Illness",
                    AnnualLimit = 30000,
                    Deductible = 500,
                    ReimbursementPercentage = 70,
                    Status = "Expired"
                },

                new Policy
                {
                    PolicyNumber = "POL003",
                    PetName = "Coco",
                    StartDate = new DateTime(2026, 1, 1),
                    EndDate = new DateTime(2026, 12, 31),
                    Coverage = "Comprehensive",
                    AnnualLimit = 40000,
                    Deductible = 750,
                    ReimbursementPercentage = 90,
                    Status = "Active"
                }
            };

            await context.Policies.AddRangeAsync(policies);

            await context.SaveChangesAsync();
        }
    }
}