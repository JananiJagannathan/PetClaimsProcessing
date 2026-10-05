using FluentAssertions;
using Moq;
using NUnit.Framework;
using PetClaims.Api.DTOs;
using PetClaims.Api.Entities;
using PetClaims.Api.Exceptions;
using PetClaims.Api.Repositories;
using PetClaims.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace PetClaims.Tests
{
    [TestFixture]
    public class ClaimServiceTests
    {
        private Mock<IClaimRepository> _claimRepositoryMock = null!;
        private Mock<IPolicyRepository> _policyRepositoryMock = null!;
        private ClaimService _service = null!;

        [SetUp]
        public void Setup()
        {
            _claimRepositoryMock =
                new Mock<IClaimRepository>();

            _policyRepositoryMock =
                new Mock<IPolicyRepository>();

            _service = new ClaimService(
                _claimRepositoryMock.Object,
                _policyRepositoryMock.Object,
                NullLogger<ClaimService>.Instance);
        }

        private static Policy CreateActivePolicy()
        {
            return new Policy
            {
                PolicyId = 1,
                PolicyNumber = "POL001",
                PetName = "Bruno",
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 12, 31),
                Coverage = "Accident and Illness",
                AnnualLimit = 50000,
                Deductible = 1000,
                ReimbursementPercentage = 80,
                Status = "Active"
            };
        }

        private static CreateClaimDto CreateValidClaimDto()
        {
            return new CreateClaimDto
            {
                PolicyNumber = "POL001",
                TreatmentDate = new DateTime(2026, 9, 15),
                Clinic = "ABC Pet Clinic",
                Diagnosis = "Fever",
                TreatmentDescription = "Consultation and medication",
                ClaimedAmount = 10000,
                InvoiceNumber = "INV001",
                DocumentName = "invoice001.pdf"
            };
        }

        private static Claim CreateUnderReviewClaim(
            string claimNumber,
            Policy policy,
            decimal amount = 10000)
        {
            return new Claim
            {
                ClaimId = 1,
                ClaimNumber = claimNumber,
                PolicyId = policy.PolicyId,
                Policy = policy,
                TreatmentDate = new DateTime(2026, 9, 15),
                Clinic = "ABC Pet Clinic",
                Diagnosis = "Fever",
                TreatmentDescription = "Treatment",
                ClaimedAmount = amount,
                ApprovedAmount = 0,
                InvoiceNumber = "INV-TEST",
                Status = "Under Review",
                CreatedDate = new DateTime(2026, 9, 15)
            };
        }

        private void SetupCommonRepositoryMethods(
            Policy policy,
            decimal alreadyApproved = 0)
        {
            _claimRepositoryMock
                .Setup(x => x.GetTotalApprovedForPolicyYearAsync(
                    policy.PolicyId,
                    2026))
                .ReturnsAsync(alreadyApproved);

            _claimRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Claim>()))
                .Returns(Task.CompletedTask);

            _claimRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Claim>()))
                .Returns(Task.CompletedTask);

            _claimRepositoryMock
                .Setup(x => x.SaveChangesAsync())
                .Returns(Task.CompletedTask);
        }

        // 1
        [Test]
        public async Task CreateClaim_ActivePolicy_Succeeds()
        {
            var policy = CreateActivePolicy();
            var dto = CreateValidClaimDto();

            _policyRepositoryMock
                .Setup(x => x.GetByPolicyNumberAsync("POL001"))
                .ReturnsAsync(policy);

            _claimRepositoryMock
                .Setup(x => x.InvoiceExistsAsync(
                    1,
                    "INV001",
                    null))
                .ReturnsAsync(false);

            SetupCommonRepositoryMethods(policy);

            var result =
                await _service.CreateClaimAsync(dto);

            result.Should().NotBeNull();
            result.PolicyNumber.Should().Be("POL001");
            result.PetName.Should().Be("Bruno");
            result.Status.Should().Be("Submitted");
            result.ClaimedAmount.Should().Be(10000);

            _claimRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Claim>()),
                Times.Once);
        }

        // 2
        [Test]
        public async Task CreateClaim_ExpiredPolicy_Fails()
        {
            var policy = CreateActivePolicy();
            policy.Status = "Expired";

            _policyRepositoryMock
                .Setup(x => x.GetByPolicyNumberAsync("POL001"))
                .ReturnsAsync(policy);

            Func<Task> action =
                () => _service.CreateClaimAsync(
                    CreateValidClaimDto());

            await action.Should()
                .ThrowAsync<BusinessException>()
                .WithMessage("Policy is not active.");
        }

        // 3
        [Test]
        public async Task CreateClaim_ZeroAmount_Fails()
        {
            var policy = CreateActivePolicy();
            var dto = CreateValidClaimDto();

            dto.ClaimedAmount = 0;

            _policyRepositoryMock
                .Setup(x => x.GetByPolicyNumberAsync("POL001"))
                .ReturnsAsync(policy);

            Func<Task> action =
                () => _service.CreateClaimAsync(dto);

            await action.Should()
                .ThrowAsync<BusinessException>();
        }

        // 4
        [Test]
        public async Task CreateClaim_DuplicateInvoice_Fails()
        {
            var policy = CreateActivePolicy();
            var dto = CreateValidClaimDto();

            _policyRepositoryMock
                .Setup(x => x.GetByPolicyNumberAsync("POL001"))
                .ReturnsAsync(policy);

            _claimRepositoryMock
                .Setup(x => x.InvoiceExistsAsync(
                    1,
                    "INV001",
                    null))
                .ReturnsAsync(true);

            Func<Task> action =
                () => _service.CreateClaimAsync(dto);

            await action.Should()
                .ThrowAsync<DuplicateInvoiceException>();
        }

        // 5
        [Test]
        public async Task CreateClaim_OutOfPeriodTreatment_Fails()
        {
            var policy = CreateActivePolicy();
            var dto = CreateValidClaimDto();

            dto.TreatmentDate =
                new DateTime(2027, 1, 10);

            _policyRepositoryMock
                .Setup(x => x.GetByPolicyNumberAsync("POL001"))
                .ReturnsAsync(policy);

            Func<Task> action =
                () => _service.CreateClaimAsync(dto);

            await action.Should()
                .ThrowAsync<BusinessException>()
                .WithMessage(
                    "Treatment date must be within the policy period.");
        }

        // 6
        [Test]
        public async Task CreateClaim_PolicyNotFound_Fails()
        {
            var dto = CreateValidClaimDto();

            _policyRepositoryMock
                .Setup(x => x.GetByPolicyNumberAsync("POL001"))
                .ReturnsAsync((Policy?)null);

            Func<Task> action =
                () => _service.CreateClaimAsync(dto);

            await action.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("Policy not found.");
        }

        // 7
        [Test]
        public async Task ApproveClaim_AppliesDeductibleAndReimbursement()
        {
            var policy = CreateActivePolicy();

            var claim =
                CreateUnderReviewClaim(
                    "CLM001",
                    policy);

            _claimRepositoryMock
                .Setup(x => x.GetByClaimNumberAsync("CLM001"))
                .ReturnsAsync(claim);

            SetupCommonRepositoryMethods(policy);

            var result =
                await _service.ApproveClaimAsync(
                    "CLM001",
                    new ApproveClaimDto
                    {
                        Reason = "Eligible treatment"
                    });

            // 10,000 - 1,000 = 9,000
            // 9,000 * 80% = 7,200
            result.ApprovedAmount.Should().Be(7200);
            result.Status.Should().Be("Approved");
        }

        // 8
        [Test]
        public async Task ApproveClaim_DoesNotExceedRemainingAnnualLimit()
        {
            var policy = CreateActivePolicy();

            var claim =
                CreateUnderReviewClaim(
                    "CLM002",
                    policy);

            _claimRepositoryMock
                .Setup(x => x.GetByClaimNumberAsync("CLM002"))
                .ReturnsAsync(claim);

            // Annual limit = 50,000
            // Already approved = 48,000
            // Remaining = 2,000
            SetupCommonRepositoryMethods(
                policy,
                48000);

            var result =
                await _service.ApproveClaimAsync(
                    "CLM002",
                    new ApproveClaimDto
                    {
                        Reason = "Annual limit applied"
                    });

            result.ApprovedAmount.Should().Be(2000);
        }

        // 9
        [Test]
        public async Task ApproveClaim_AmountAboveMaximum_Fails()
        {
            var policy = CreateActivePolicy();

            var claim =
                CreateUnderReviewClaim(
                    "CLM003",
                    policy);

            _claimRepositoryMock
                .Setup(x => x.GetByClaimNumberAsync("CLM003"))
                .ReturnsAsync(claim);

            SetupCommonRepositoryMethods(policy);

            Func<Task> action =
                () => _service.ApproveClaimAsync(
                    "CLM003",
                    new ApproveClaimDto
                    {
                        RequestedApprovedAmount = 15000,
                        Reason = "Too high"
                    });

            await action.Should()
                .ThrowAsync<BusinessException>();
        }

        // 10
        [Test]
        public async Task RejectClaim_WithoutReason_Fails()
        {
            var policy = CreateActivePolicy();

            var claim =
                CreateUnderReviewClaim(
                    "CLM004",
                    policy);

            _claimRepositoryMock
                .Setup(x => x.GetByClaimNumberAsync("CLM004"))
                .ReturnsAsync(claim);

            Func<Task> action =
                () => _service.RejectClaimAsync(
                    "CLM004",
                    new RejectClaimDto
                    {
                        Reason = ""
                    });

            await action.Should()
                .ThrowAsync<BusinessException>()
                .WithMessage(
                    "Rejection reason is required.");
        }

        // 11
        [Test]
        public async Task ApproveClaim_FinalisedClaim_Fails()
        {
            var policy = CreateActivePolicy();

            var claim =
                CreateUnderReviewClaim(
                    "CLM005",
                    policy);

            claim.Status = "Approved";

            _claimRepositoryMock
                .Setup(x => x.GetByClaimNumberAsync("CLM005"))
                .ReturnsAsync(claim);

            Func<Task> action =
                () => _service.ApproveClaimAsync(
                    "CLM005",
                    new ApproveClaimDto());

            await action.Should()
                .ThrowAsync<BusinessException>()
                .WithMessage(
                    "A finalised claim cannot be processed again.");
        }

        // 12
        [Test]
        public async Task MoveToReview_SubmittedClaim_Succeeds()
        {
            var policy = CreateActivePolicy();

            var claim = new Claim
            {
                ClaimId = 1,
                ClaimNumber = "CLM006",
                PolicyId = 1,
                Policy = policy,
                TreatmentDate = new DateTime(2026, 9, 15),
                ClaimedAmount = 10000,
                Status = "Submitted"
            };

            _claimRepositoryMock
                .Setup(x => x.GetByClaimNumberAsync("CLM006"))
                .ReturnsAsync(claim);

            SetupCommonRepositoryMethods(policy);

            var result =
                await _service.MoveToReviewAsync(
                    "CLM006");

            result.Status.Should().Be("Under Review");

            claim.ClaimDecisions
                .Should().ContainSingle();

            claim.ClaimDecisions.First()
                .Decision.Should().Be("Under Review");
        }

        [Test]
public async Task RequestMoreInformation_UnderReviewClaim_Succeeds()
{
    var policy = CreateActivePolicy();

    var claim = CreateUnderReviewClaim(
        "CLM-MOREINFO",
        policy);

    _claimRepositoryMock
        .Setup(x =>
            x.GetByClaimNumberAsync("CLM-MOREINFO"))
        .ReturnsAsync(claim);

    SetupCommonRepositoryMethods(policy);

    var result =
        await _service.RequestMoreInformationAsync(
            "CLM-MOREINFO",
            new MoreInformationDto
            {
                Reason = "Detailed invoice required."
            });

    result.Status
        .Should()
        .Be("More Information Required");

    claim.ClaimDecisions
        .Should()
        .Contain(x =>
            x.Decision == "More Information Required");
}

[Test]
public async Task ApproveClaim_LowerAmount_ReturnsPartiallyApproved()
{
    var policy = CreateActivePolicy();

    var claim = CreateUnderReviewClaim(
        "CLM-PARTIAL",
        policy);

    _claimRepositoryMock
        .Setup(x =>
            x.GetByClaimNumberAsync("CLM-PARTIAL"))
        .ReturnsAsync(claim);

    SetupCommonRepositoryMethods(policy);

    var result =
        await _service.ApproveClaimAsync(
            "CLM-PARTIAL",
            new ApproveClaimDto
            {
                RequestedApprovedAmount = 3000,
                Reason = "Only part of treatment is eligible."
            });

    result.Status
        .Should()
        .Be("Partially Approved");

    result.ApprovedAmount
        .Should()
        .Be(3000);
}
    }
}