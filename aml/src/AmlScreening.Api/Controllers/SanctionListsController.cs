using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.SanctionLists;
using AmlScreening.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmlScreening.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class SanctionListsController : ControllerBase
{
    private const long MaxFileSizeBytes = 100 * 1024 * 1024; // 100MB for large UN list

    private readonly ISanctionListUploadService _uploadService;

    public SanctionListsController(ISanctionListUploadService uploadService)
    {
        _uploadService = uploadService;
    }

    [HttpGet("sources")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SanctionListSourceDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSources(CancellationToken cancellationToken)
    {
        var result = await _uploadService.GetSourcesAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("entries")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SanctionListEntryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEntries(
        [FromQuery] string? searchTerm,
        [FromQuery] string? listSource,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _uploadService.GetEntriesAsync(searchTerm, listSource, pageNumber, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpPost("entries")]
    [ProducesResponseType(typeof(ApiResponse<SanctionListEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEntry(
        [FromBody] CreateSanctionListEntryDto dto,
        CancellationToken cancellationToken = default)
    {
        var result = await _uploadService.CreateEntryAsync(dto, cancellationToken);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("entries")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteByListSource(
        [FromQuery] string listSource,
        CancellationToken cancellationToken = default)
    {
        var result = await _uploadService.DeleteByListSourceAsync(listSource, cancellationToken);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("reindex")]
    [ProducesResponseType(typeof(ApiResponse<long>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Reindex(CancellationToken cancellationToken)
    {
        var result = await _uploadService.ReindexAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<SanctionListUploadResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(
        IFormFile file,
        [FromForm] string listSource,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<SanctionListUploadResultDto>.Fail("No file uploaded."));
        if (file.Length > MaxFileSizeBytes)
            return BadRequest(ApiResponse<SanctionListUploadResultDto>.Fail("File size must be less than 100MB."));
        if (string.IsNullOrWhiteSpace(listSource))
            return BadRequest(ApiResponse<SanctionListUploadResultDto>.Fail("List source is required."));

        await using var stream = file.OpenReadStream();
        var result = await _uploadService.UploadAsync(listSource.Trim(), stream, file.FileName, cancellationToken);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
}
