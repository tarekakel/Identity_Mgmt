using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.CorporateBulkUpload;
using AmlScreening.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmlScreening.Api.Controllers;

[ApiController]
[Route("api/corporate-bulk-upload")]
[AllowAnonymous]
public class CorporateBulkUploadController : ControllerBase
{
    private const long MaxUploadBytes = 15 * 1024 * 1024;

    private readonly ICorporateBulkUploadService _service;

    public CorporateBulkUploadController(ICorporateBulkUploadService service)
    {
        _service = service;
    }

    [HttpGet("sample")]
    public IActionResult DownloadSample()
    {
        var bytes = _service.GetSampleWorkbookBytes();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Data-Customer_Bulk_upload_Sample_Corporate.xlsx");
    }

    [HttpPost("upload")]
    [RequestSizeLimit(MaxUploadBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadBytes)]
    [ProducesResponseType(typeof(ApiResponse<CorporateBulkUploadResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CorporateBulkUploadResultDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload([FromForm] CorporateBulkUploadUploadForm form, CancellationToken cancellationToken)
    {
        if (form.File == null || form.File.Length == 0)
            return BadRequest(ApiResponse<CorporateBulkUploadResultDto>.Fail("File is required."));

        await using var stream = form.File.OpenReadStream();
        var options = new CorporateBulkUploadOptionsDto
        {
            MatchThreshold = form.MatchThreshold,
            CheckPepUkOnly = form.CheckPepUkOnly,
            CheckDisqualifiedDirectorUkOnly = form.CheckDisqualifiedDirectorUkOnly,
            CheckSanctions = form.CheckSanctions,
            CheckProfileOfInterest = form.CheckProfileOfInterest,
            CheckReputationalRiskExposure = form.CheckReputationalRiskExposure,
            CheckRegulatoryEnforcementList = form.CheckRegulatoryEnforcementList,
            CheckInsolvencyUkIreland = form.CheckInsolvencyUkIreland
        };

        var result = await _service.UploadAsync(stream, form.File.FileName, options, cancellationToken);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("batches")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CorporateBulkUploadBatchListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBatches([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? uploadedBy, CancellationToken cancellationToken)
    {
        var result = await _service.GetBatchesAsync(from, to, uploadedBy, cancellationToken);
        return Ok(result);
    }

    [HttpGet("batches/{batchId:guid}/lines")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CorporateBulkUploadLineDetailDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBatchLines(Guid batchId, [FromQuery] string? caseStatus, CancellationToken cancellationToken)
    {
        var result = await _service.GetBatchLinesAsync(batchId, caseStatus, cancellationToken);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }
}

public class CorporateBulkUploadUploadForm
{
    public IFormFile? File { get; set; }
    public int MatchThreshold { get; set; } = 85;
    public bool CheckPepUkOnly { get; set; }
    public bool CheckDisqualifiedDirectorUkOnly { get; set; }
    public bool CheckSanctions { get; set; }
    public bool CheckProfileOfInterest { get; set; }
    public bool CheckReputationalRiskExposure { get; set; }
    public bool CheckRegulatoryEnforcementList { get; set; }
    public bool CheckInsolvencyUkIreland { get; set; }
}
