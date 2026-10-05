using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using NUnit.Framework;
using PetClaims.Api.DTOs;

namespace PetClaims.Tests
{
    [TestFixture]
    public class ClaimValidationTests
    {
        private static List<ValidationResult> Validate(
            object model)
        {
            var results =
                new List<ValidationResult>();

            var context =
                new ValidationContext(model);

            Validator.TryValidateObject(
                model,
                context,
                results,
                true);

            return results;
        }

        [Test]
        public void CreateClaim_ZeroAmount_IsInvalid()
        {
            var dto = CreateValidDto();

            dto.ClaimedAmount = 0;

            var results = Validate(dto);

            results.Should().NotBeEmpty();

            results.Should().Contain(
                x => x.ErrorMessage ==
                    "Claimed amount must be greater than zero.");
        }

        [Test]
        public void CreateClaim_ValidData_IsValid()
        {
            var dto = CreateValidDto();

            var results = Validate(dto);

            results.Should().BeEmpty();
        }

        [Test]
        public void RejectClaim_EmptyReason_IsInvalid()
        {
            var dto = new RejectClaimDto
            {
                Reason = ""
            };

            var results = Validate(dto);

            results.Should().NotBeEmpty();
        }

        private static CreateClaimDto CreateValidDto()
        {
            return new CreateClaimDto
            {
                PolicyNumber = "POL001",
                TreatmentDate =
                    new DateTime(2026, 9, 15),
                Clinic = "ABC Clinic",
                Diagnosis = "Fever",
                TreatmentDescription = "Treatment",
                ClaimedAmount = 10000,
                InvoiceNumber = "INV001"
            };
        }
    }
}