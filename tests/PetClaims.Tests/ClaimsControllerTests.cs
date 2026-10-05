using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using PetClaims.Api.Controllers;
using PetClaims.Api.DTOs;
using PetClaims.Api.Services;

namespace PetClaims.Tests
{
    [TestFixture]
    public class ClaimsControllerTests
    {
        private Mock<IClaimService> _serviceMock = null!;
        private ClaimsController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _serviceMock =
                new Mock<IClaimService>();

            _controller =
                new ClaimsController(
                    _serviceMock.Object);
        }

        [Test]
        public async Task CreateClaim_ValidRequest_Returns201()
        {
            var request = new CreateClaimDto
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

            var response = new ClaimDetailsDto
            {
                ClaimNumber = "CLM001",
                PolicyNumber = "POL001",
                Status = "Submitted",
                ClaimedAmount = 10000
            };

            _serviceMock
                .Setup(x => x.CreateClaimAsync(request))
                .ReturnsAsync(response);

            var action =
                await _controller.CreateClaim(request);

            var result =
                action.Result.Should()
                    .BeOfType<CreatedAtActionResult>()
                    .Subject;

            result.StatusCode.Should().Be(201);
        }

        [Test]
        public async Task GetClaim_MissingClaim_Returns404()
        {
            _serviceMock
                .Setup(x =>
                    x.GetClaimAsync("CLM999"))
                .ReturnsAsync(
                    (ClaimDetailsDto?)null);

            var action =
                await _controller.GetClaim(
                    "CLM999");

            action.Result.Should()
                .BeOfType<NotFoundObjectResult>();
        }

        [Test]
        public async Task ApproveClaim_ValidRequest_Returns200()
        {
            var request =
                new ApproveClaimDto
                {
                    Reason = "Eligible"
                };

            var response =
                new ClaimDetailsDto
                {
                    ClaimNumber = "CLM001",
                    Status = "Approved",
                    ApprovedAmount = 7200
                };

            _serviceMock
                .Setup(x =>
                    x.ApproveClaimAsync(
                        "CLM001",
                        request))
                .ReturnsAsync(response);

            var action =
                await _controller.ApproveClaim(
                    "CLM001",
                    request);

            action.Result.Should()
                .BeOfType<OkObjectResult>();
        }
    }
}