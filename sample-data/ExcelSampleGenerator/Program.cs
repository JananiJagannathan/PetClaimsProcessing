using ClosedXML.Excel;

// Create a new Excel workbook
using var workbook = new XLWorkbook();

// Create a worksheet named "Claims"
var sheet = workbook.Worksheets.Add("Claims");

// --------------------------------------------------
// COLUMN HEADERS
// --------------------------------------------------

sheet.Cell(1, 1).Value = "PolicyNumber";
sheet.Cell(1, 2).Value = "TreatmentDate";
sheet.Cell(1, 3).Value = "Clinic";
sheet.Cell(1, 4).Value = "Diagnosis";
sheet.Cell(1, 5).Value = "TreatmentDescription";
sheet.Cell(1, 6).Value = "ClaimedAmount";
sheet.Cell(1, 7).Value = "InvoiceNumber";
sheet.Cell(1, 8).Value = "DocumentName";

// --------------------------------------------------
// SAMPLE CLAIM 1
// --------------------------------------------------

sheet.Cell(2, 1).Value = "POL001";
sheet.Cell(2, 2).Value = new DateTime(2026, 10, 1);
sheet.Cell(2, 3).Value = "Happy Paws Clinic";
sheet.Cell(2, 4).Value = "Fever";
sheet.Cell(2, 5).Value = "Consultation and medication";
sheet.Cell(2, 6).Value = 5000;
sheet.Cell(2, 7).Value = "EXCEL-INV-001";
sheet.Cell(2, 8).Value = "excel-invoice-1.pdf";

// --------------------------------------------------
// SAMPLE CLAIM 2
// --------------------------------------------------

sheet.Cell(3, 1).Value = "POL003";
sheet.Cell(3, 2).Value = new DateTime(2026, 10, 2);
sheet.Cell(3, 3).Value = "City Pet Clinic";
sheet.Cell(3, 4).Value = "Injury";
sheet.Cell(3, 5).Value = "Injury treatment";
sheet.Cell(3, 6).Value = 7500;
sheet.Cell(3, 7).Value = "EXCEL-INV-002";
sheet.Cell(3, 8).Value = "excel-invoice-2.pdf";

// Adjust column widths automatically
sheet.Columns().AdjustToContents();

// --------------------------------------------------
// SAVE THE FILE
// --------------------------------------------------

var filePath = Path.Combine(
    Directory.GetCurrentDirectory(),
    "PetClaimsSample.xlsx");

workbook.SaveAs(filePath);

Console.WriteLine("Excel sample file created successfully.");
Console.WriteLine($"File: {filePath}");