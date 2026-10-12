using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using PetClaims.Api.Data;
using PetClaims.Api.Entities;
using PetClaims.Api.Exceptions;

namespace PetClaims.Api.Services
{
    public class ExcelService : IExcelService
    {
        private readonly AppDbContext _context;

        public ExcelService(AppDbContext context)
        {
            _context = context;
        }

        // --------------------------------------------------
        // IMPORT CLAIMS FROM EXCEL
        // --------------------------------------------------

        public async Task<int> ImportClaimsAsync(
            Stream fileStream)
        {
            using var workbook =
                new XLWorkbook(fileStream);

            foreach (var ws in workbook.Worksheets)
{
    Console.WriteLine($"Sheet Name: '{ws.Name}'");
}

var worksheet = workbook.Worksheets
    .FirstOrDefault(w =>
        w.Name.Trim().Equals(
            "Claims",
            StringComparison.OrdinalIgnoreCase));

if (worksheet == null)
{
    throw new BusinessException(
        "Workbook must contain a Claims sheet.");
}

            var importedCount = 0;

            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                var policyNumber =
                    row.Cell(1).GetString().Trim();

                var treatmentDate =
                    row.Cell(2).GetDateTime();

                var clinic =
                    row.Cell(3).GetString().Trim();

                var diagnosis =
                    row.Cell(4).GetString().Trim();

                var treatmentDescription =
                    row.Cell(5).GetString().Trim();

                var claimedAmount =
                    row.Cell(6).GetValue<decimal>();

                var invoiceNumber =
                    row.Cell(7).GetString().Trim();

                var documentName =
                    row.Cell(8).GetString().Trim();
                
                // ------------------------------------------
                // Validate Policy
                // ------------------------------------------

                var policy =
                    await _context.Policies
                        .FirstOrDefaultAsync(
                            p => p.PolicyNumber ==
                                 policyNumber);

                if (policy == null)
                {
                    throw new BusinessException(
                        $"Policy {policyNumber} was not found.");
                }

                if (!policy.Status.Equals(
                        "Active",
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new BusinessException(
                        $"Policy {policyNumber} is not active.");
                }

                // ------------------------------------------
                // Validate Treatment Date
                // ------------------------------------------

                if (treatmentDate.Date <
                        policy.StartDate.Date ||
                    treatmentDate.Date >
                        policy.EndDate.Date)
                {
                    throw new BusinessException(
                        $"Treatment date is outside the policy period for {policyNumber}.");
                }

                // ------------------------------------------
                // Validate Amount
                // ------------------------------------------

                if (claimedAmount <= 0)
                {
                    throw new BusinessException(
                        "Claimed amount must be greater than zero.");
                }

                // ------------------------------------------
                // Duplicate Invoice
                // ------------------------------------------

                var duplicateInvoice =
                    await _context.Claims.AnyAsync(
                        c =>
                            c.PolicyId == policy.PolicyId &&
                            c.InvoiceNumber == invoiceNumber);

                if (duplicateInvoice)
                {
                    throw new DuplicateInvoiceException(
                        $"Duplicate invoice {invoiceNumber} for policy {policyNumber}.");
                }

                // ------------------------------------------
                // Create Claim
                // ------------------------------------------

                var claim = new Claim
                {
                    ClaimNumber =
                        GenerateClaimNumber(),

                    PolicyId =
                        policy.PolicyId,

                    TreatmentDate =
                        treatmentDate,

                    Clinic =
                        clinic,

                    Diagnosis =
                        diagnosis,

                    TreatmentDescription =
                        treatmentDescription,

                    ClaimedAmount =
                        claimedAmount,

                    ApprovedAmount =
                        0,

                    InvoiceNumber =
                        invoiceNumber,

                    Status =
                        "Submitted",

                    CreatedDate =
                        DateTime.UtcNow
                };

                // ------------------------------------------
                // Document Metadata
                // ------------------------------------------

                if (!string.IsNullOrWhiteSpace(
                        documentName))
                {
                    claim.ClaimDocuments.Add(
                        new ClaimDocument
                        {
                            FileName =
                                documentName,

                            DocumentType =
                                "Invoice",

                            UploadedDate =
                                DateTime.UtcNow
                        });
                }

                await _context.Claims.AddAsync(claim);

                importedCount++;
            }

            await _context.SaveChangesAsync();

            return importedCount;
        }

        // --------------------------------------------------
        // EXPORT DATABASE TO EXCEL
        // --------------------------------------------------

        public async Task<byte[]> ExportClaimsAsync()
        {
            var policies =
                await _context.Policies
                    .AsNoTracking()
                    .ToListAsync();

            var claims =
                await _context.Claims
                    .Include(c => c.Policy)
                    .AsNoTracking()
                    .ToListAsync();


            var claimDecisions =
                await _context.ClaimDecisions
                    .AsNoTracking()
                    .ToListAsync();

            var claimDocuments =
                await _context.ClaimDocuments
                    .AsNoTracking()
                    .ToListAsync();

            using var workbook = new XLWorkbook();

            CreatePoliciesSheet(
                workbook,
                policies);

            CreateClaimsSheet(
                workbook,
                claims);


            CreateClaimDecisionsSheet(
                workbook,
                claimDecisions,
                claims);

            CreateClaimDocumentsSheet(
                workbook,
                claimDocuments,
                claims);

            using var stream =
                new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();
        }

        

        // --------------------------------------------------
        // POLICIES SHEET
        // --------------------------------------------------

        private static void CreatePoliciesSheet(
            XLWorkbook workbook,
            List<Policy> policies)
        {
            var sheet =
                workbook.Worksheets.Add(
                    "Policies");

            sheet.Cell(1, 1).Value =
                "PolicyNumber";

            sheet.Cell(1, 2).Value =
                "PetName";

            sheet.Cell(1, 3).Value =
                "StartDate";

            sheet.Cell(1, 4).Value =
                "EndDate";

            sheet.Cell(1, 5).Value =
                "Coverage";

            sheet.Cell(1, 6).Value =
                "AnnualLimit";

            sheet.Cell(1, 7).Value =
                "Deductible";

            sheet.Cell(1, 8).Value =
                "ReimbursementPercentage";

            sheet.Cell(1, 9).Value =
                "Status";

            var row = 2;

            foreach (var policy in policies)
            {
                sheet.Cell(row, 1).Value =
                    policy.PolicyNumber;

                sheet.Cell(row, 2).Value =
                    policy.PetName;

                sheet.Cell(row, 3).Value =
                    policy.StartDate;

                sheet.Cell(row, 4).Value =
                    policy.EndDate;

                sheet.Cell(row, 5).Value =
                    policy.Coverage;

                sheet.Cell(row, 6).Value =
                    policy.AnnualLimit;

                sheet.Cell(row, 7).Value =
                    policy.Deductible;

                sheet.Cell(row, 8).Value =
                    policy.ReimbursementPercentage;

                sheet.Cell(row, 9).Value =
                    policy.Status;

                row++;
            }

            sheet.Columns().AdjustToContents();
        }

        // --------------------------------------------------
        // CLAIMS SHEET
        // --------------------------------------------------

        private static void CreateClaimsSheet(
            XLWorkbook workbook,
            List<Claim> claims)
        {
            var sheet =
                workbook.Worksheets.Add(
                    "Claims");

            sheet.Cell(1, 1).Value =
                "ClaimNumber";

            sheet.Cell(1, 2).Value =
                "PolicyNumber";

            sheet.Cell(1, 3).Value =
                "PetName";

            sheet.Cell(1, 4).Value =
                "TreatmentDate";

            sheet.Cell(1, 5).Value =
                "Clinic";

                        sheet.Cell(1, 6).Value =
                "Diagnosis";

            sheet.Cell(1, 7).Value =
                "TreatmentDescription";

            sheet.Cell(1, 8).Value =
                "ClaimedAmount";

            sheet.Cell(1, 9).Value =
                "ApprovedAmount";

            sheet.Cell(1, 10).Value =
                "InvoiceNumber";

            sheet.Cell(1, 11).Value =
                "Status";

            sheet.Cell(1, 12).Value =
                "CreatedDate";

            var row = 2;

            foreach (var claim in claims)
            {
                sheet.Cell(row, 1).Value =
                    claim.ClaimNumber;

                sheet.Cell(row, 2).Value =
                    claim.Policy?.PolicyNumber ?? "";

                sheet.Cell(row, 3).Value =
                    claim.Policy?.PetName ?? "";

                sheet.Cell(row, 4).Value =
                    claim.TreatmentDate;

                sheet.Cell(row, 5).Value =
                    claim.Clinic;

                sheet.Cell(row, 6).Value =
                    claim.Diagnosis;

                sheet.Cell(row, 7).Value =
                    claim.TreatmentDescription;

                sheet.Cell(row, 8).Value =
                    claim.ClaimedAmount;

                sheet.Cell(row, 9).Value =
                    claim.ApprovedAmount;

                sheet.Cell(row, 10).Value =
                    claim.InvoiceNumber;

                sheet.Cell(row, 11).Value =
                    claim.Status;

                sheet.Cell(row, 12).Value =
                    claim.CreatedDate;

                row++;
            }

            sheet.Columns().AdjustToContents();
        }



        // --------------------------------------------------
        // CLAIM DECISIONS SHEET
        // --------------------------------------------------

        private static void CreateClaimDecisionsSheet(
            XLWorkbook workbook,
            List<ClaimDecision> decisions,
            List<Claim> claims)
        {
            var sheet =
                workbook.Worksheets.Add(
                    "ClaimDecisions");

            sheet.Cell(1, 1).Value =
                "ClaimNumber";

            sheet.Cell(1, 2).Value =
                "Decision";

            sheet.Cell(1, 3).Value =
                "DecisionDate";

            sheet.Cell(1, 4).Value =
                "Reason";

            sheet.Cell(1, 5).Value =
                "ApprovedAmount";

            var row = 2;

            foreach (var decision in decisions)
            {
                var claim =
                    claims.FirstOrDefault(
                        c => c.ClaimId ==
                             decision.ClaimId);

                sheet.Cell(row, 1).Value =
                    claim?.ClaimNumber ?? "";

                sheet.Cell(row, 2).Value =
                    decision.Decision;

                sheet.Cell(row, 3).Value =
                    decision.DecisionDate;

                sheet.Cell(row, 4).Value =
                    decision.Reason;

                sheet.Cell(row, 5).Value =
                    decision.ApprovedAmount;

                row++;
            }

            sheet.Columns().AdjustToContents();
        }

        // --------------------------------------------------
        // CLAIM DOCUMENTS SHEET
        // --------------------------------------------------

        private static void CreateClaimDocumentsSheet(
            XLWorkbook workbook,
            List<ClaimDocument> documents,
            List<Claim> claims)
        {
            var sheet =
                workbook.Worksheets.Add(
                    "ClaimDocuments");

            sheet.Cell(1, 1).Value =
                "ClaimNumber";

            sheet.Cell(1, 2).Value =
                "FileName";

            sheet.Cell(1, 3).Value =
                "DocumentType";

            sheet.Cell(1, 4).Value =
                "UploadedDate";

            var row = 2;

            foreach (var document in documents)
            {
                var claim =
                    claims.FirstOrDefault(
                        c => c.ClaimId ==
                             document.ClaimId);

                sheet.Cell(row, 1).Value =
                    claim?.ClaimNumber ?? "";

                sheet.Cell(row, 2).Value =
                    document.FileName;

                sheet.Cell(row, 3).Value =
                    document.DocumentType;

                sheet.Cell(row, 4).Value =
                    document.UploadedDate;

                row++;
            }

            sheet.Columns().AdjustToContents();
        }

        // --------------------------------------------------
        // CLAIM NUMBER GENERATOR
        // --------------------------------------------------

        private static string GenerateClaimNumber()
        {
            return $"CLM-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid().ToString("N")[..4]}";
        }
    }
}