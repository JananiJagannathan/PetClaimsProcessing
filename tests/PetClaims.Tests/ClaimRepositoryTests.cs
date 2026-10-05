using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using PetClaims.Api.Data;
using PetClaims.Api.DTOs;
using PetClaims.Api.Entities;
using PetClaims.Api.Repositories;

namespace PetClaims.Tests
{
    [TestFixture]
    public class ClaimRepositoryTests
    {
        private SqliteConnection _connection = null!;
        private AppDbContext _context = null!;
        private ClaimRepository _repository = null!;

        [SetUp]
        public async Task Setup()
        {
            _connection =
                new SqliteConnection(
                    "DataSource=:memory:");

            await _connection.OpenAsync();

            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlite(_connection)
                    .Options;

            _context =
                new AppDbContext(options);

            await _context.Database
                .EnsureCreatedAsync();

            _repository =
                new ClaimRepository(_context);

            await SeedDataAsync();
        }

        [TearDown]
        public async Task TearDown()
        {
            await _context.DisposeAsync();
            await _connection.DisposeAsync();
        }

        private async Task SeedDataAsync()
        {
            var policy1 = new Policy
            {
                PolicyNumber = "POL001",
                PetName = "Bruno",
                StartDate =
                    new DateTime(2026, 1, 1),
                EndDate =
                    new DateTime(2026, 12, 31),
                Coverage =
                    "Accident and Illness",
                AnnualLimit = 50000,
                Deductible = 1000,
                ReimbursementPercentage = 80,
                Status = "Active"
            };

            var policy2 = new Policy
            {
                PolicyNumber = "POL002",
                PetName = "Coco",
                StartDate =
                    new DateTime(2026, 1, 1),
                EndDate =
                    new DateTime(2026, 12, 31),
                Coverage = "Comprehensive",
                AnnualLimit = 40000,
                Deductible = 500,
                ReimbursementPercentage = 90,
                Status = "Active"
            };

            _context.Policies.AddRange(
                policy1,
                policy2);

            await _context.SaveChangesAsync();

            _context.Claims.AddRange(
                new Claim
                {
                    ClaimNumber = "CLM001",
                    PolicyId = policy1.PolicyId,
                    TreatmentDate =
                        new DateTime(2026, 9, 10),
                    Clinic = "ABC Clinic",
                    Diagnosis = "Fever",
                    TreatmentDescription = "Treatment",
                    ClaimedAmount = 10000,
                    ApprovedAmount = 7200,
                    InvoiceNumber = "INV001",
                    Status = "Approved",
                    CreatedDate =
                        new DateTime(2026, 9, 10)
                },

                new Claim
                {
                    ClaimNumber = "CLM002",
                    PolicyId = policy1.PolicyId,
                    TreatmentDate =
                        new DateTime(2026, 9, 20),
                    Clinic = "XYZ Clinic",
                    Diagnosis = "Injury",
                    TreatmentDescription = "Treatment",
                    ClaimedAmount = 5000,
                    ApprovedAmount = 0,
                    InvoiceNumber = "INV002",
                    Status = "Submitted",
                    CreatedDate =
                        new DateTime(2026, 9, 20)
                },

                new Claim
                {
                    ClaimNumber = "CLM003",
                    PolicyId = policy2.PolicyId,
                    TreatmentDate =
                        new DateTime(2026, 10, 5),
                    Clinic = "ABC Clinic",
                    Diagnosis = "Injury",
                    TreatmentDescription = "Surgery",
                    ClaimedAmount = 25000,
                    ApprovedAmount = 15000,
                    InvoiceNumber = "INV003",
                    Status = "Approved",
                    CreatedDate =
                        new DateTime(2026, 10, 5)
                });

            await _context.SaveChangesAsync();
        }

        [Test]
        public async Task Dashboard_ReturnsCorrectTotals()
        {
            var result =
                await _repository.GetDashboardAsync();

            result.ApprovedCount.Should().Be(2);
            result.SubmittedCount.Should().Be(1);

            result.TotalClaimedAmount
                .Should().Be(40000);

            result.TotalApprovedAmount
                .Should().Be(22200);
        }

        [Test]
        public async Task MonthlyReport_GroupsClaimsCorrectly()
        {
            var result =
                await _repository
                    .GetMonthlyReportAsync();

            result.Should().HaveCount(2);

            var september =
                result.Single(
                    x => x.Month == 9);

            september.ClaimCount.Should().Be(2);

            september.TotalClaimedAmount
                .Should().Be(15000);

            september.AverageClaimAmount
                .Should().Be(7500);
        }

        [Test]
        public async Task Search_StatusFilter_ReturnsApprovedClaims()
        {
            var result =
                await _repository.SearchAsync(
                    new ClaimSearchDto
                    {
                        Status = "Approved"
                    });

            result.Should().HaveCount(2);

            result.Should().OnlyContain(
                x => x.Status == "Approved");
        }

        [Test]
        public async Task Search_ClinicFilter_ReturnsCorrectClaims()
        {
            var result =
                await _repository.SearchAsync(
                    new ClaimSearchDto
                    {
                        Clinic = "ABC"
                    });

            result.Should().HaveCount(2);
        }

        [Test]
        public async Task InvoiceExists_ExistingInvoice_ReturnsTrue()
        {
            var result =
                await _repository
                    .InvoiceExistsAsync(
                        1,
                        "INV001");

            result.Should().BeTrue();
        }
    }
}