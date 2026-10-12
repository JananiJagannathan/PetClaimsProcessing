using Microsoft.EntityFrameworkCore;
using PetClaims.Api.Entities;

namespace PetClaims.Api.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
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
                    PetType = "Dog",
                    Breed = "Labrador",

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
                    PetType = "Cat",
                    Breed = "Persian",

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
                    PetType = "Dog",
                    Breed = "Golden Retriever",

                    StartDate = new DateTime(2026, 1, 1),
                    EndDate = new DateTime(2026, 12, 31),

                    Coverage = "Comprehensive",
                    AnnualLimit = 40000,
                    Deductible = 750,
                    ReimbursementPercentage = 90,
                    Status = "Active"
                },
                new Policy
{
    PolicyNumber = "POL004",
    PetName = "Snowy",
    PetType = "Rabbit",
    Breed = "Holland Lop",

    StartDate = new DateTime(2026, 1, 1),
    EndDate = new DateTime(2026, 12, 31),

    Coverage = "Comprehensive",
    AnnualLimit = 45000,
    Deductible = 800,
    ReimbursementPercentage = 85,
    Status = "Active"
},
new Policy
{
    PolicyNumber = "POL005",
    PetName = "Rio",
    PetType = "Parrot",
    Breed = "Macaw",

    StartDate = new DateTime(2026, 1, 1),
    EndDate = new DateTime(2026, 12, 31),

    Coverage = "Accident and Illness",
    AnnualLimit = 35000,
    Deductible = 600,
    ReimbursementPercentage = 75,
    Status = "Active"
},
new Policy
{
    PolicyNumber = "POL006",
    PetName = "Lily",
    PetType = "Cat",
    Breed = "Persian",

    StartDate = new DateTime(2026, 1, 1),
    EndDate = new DateTime(2026, 12, 31),

    Coverage = "Comprehensive",
    AnnualLimit = 55000,
    Deductible = 900,
    ReimbursementPercentage = 85,
    Status = "Active"
}

            };

            await context.Policies.AddRangeAsync(policies);

            await context.SaveChangesAsync();
        }
    }
}