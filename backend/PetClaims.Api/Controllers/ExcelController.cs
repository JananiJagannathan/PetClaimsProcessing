using Microsoft.AspNetCore.Mvc;
using PetClaims.Api.Services;

namespace PetClaims.Api.Controllers
{
    [ApiController]
    [Route("api/excel/claims")]
    public class ExcelController : ControllerBase
    {
        private readonly IExcelService _excelService;

        public ExcelController(
            IExcelService excelService)
        {
            _excelService = excelService;
        }

        // --------------------------------------------------
        // IMPORT EXCEL
        // POST /api/excel/claims/import
        // --------------------------------------------------

        [HttpPost("import")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportClaims(
            IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new
                {
                    message = "Please select an Excel file."
                });
            }

            var extension =
                Path.GetExtension(file.FileName);

            if (!extension.Equals(
                    ".xlsx",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    message = "Only .xlsx files are supported."
                });
            }

            await using var stream =
                file.OpenReadStream();

            var importedCount =
                await _excelService
                    .ImportClaimsAsync(stream);

            return Ok(new
            {
                message =
                    "Excel import completed successfully.",

                importedClaims =
                    importedCount
            });
        }

        // --------------------------------------------------
        // EXPORT EXCEL
        // GET /api/excel/claims/export
        // --------------------------------------------------

        [HttpGet("export")]
        public async Task<IActionResult> ExportClaims()
        {
            var fileBytes =
                await _excelService
                    .ExportClaimsAsync();

            var fileName =
                $"PetClaimsExport-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
    }
}
