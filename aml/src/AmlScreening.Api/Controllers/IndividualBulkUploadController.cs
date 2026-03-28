using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.IndividualBulkUpload;
using AmlScreening.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmlScreening.Api.Controllers;

[ApiController]
[Route("api/individual-bulk-upload")]
[AllowAnonymous]
public class IndividualBulkUploadController : ControllerBase
{
    private const long MaxUploadBytes = 15 * 1024 * 1024;

    private readonly IIndividualBulkUploadService _service;

    public IndividualBulkUploadController(IIndividualBulkUploadService service)
    {
        _service = service;
    }

    [HttpGet("sample")]
    public IActionResult DownloadSample()
    {
        var bytes = _service.GetSampleWorkbookBytes();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Data-Customer_Bulk_upload_Sample_Individual.xlsx");
    }

    [HttpPost("upload")]
    [RequestSizeLimit(MaxUploadBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadBytes)]
    [ProducesResponseType(typeof(ApiResponse<IndividualBulkUploadResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IndividualBulkUploadResultDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload([FromForm] IndividualBulkUploadUploadForm form, CancellationToken cancellationToken)
    {
        if (form.File == null || form.File.Length == 0)
            return BadRequest(ApiResponse<IndividualBulkUploadResultDto>.Fail("File is required."));

        await using var stream = form.File.OpenReadStream();
        var options = new IndividualBulkUploadOptionsDto
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
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<IndividualBulkUploadBatchListItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBatches([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? uploadedBy, CancellationToken cancellationToken)
    {
        var result = await _service.GetBatchesAsync(from, to, uploadedBy, cancellationToken);
        return Ok(result);
    }

    [HttpGet("batches/{batchId:guid}/lines")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<IndividualBulkUploadLineDetailDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBatchLines(Guid batchId, [FromQuery] string? caseStatus, CancellationToken cancellationToken)
    {
        var result = await _service.GetBatchLinesAsync(batchId, caseStatus, cancellationToken);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }
}

public class IndividualBulkUploadUploadForm
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
