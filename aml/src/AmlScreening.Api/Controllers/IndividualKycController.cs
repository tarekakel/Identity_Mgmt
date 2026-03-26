using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.IndividualKyc;
using AmlScreening.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmlScreening.Api.Controllers;

[ApiController]
[Route("api/individual-kyc")]
[AllowAnonymous]
public class IndividualKycController : ControllerBase
{
    private const long MaxDocumentSizeBytes = 10 * 1024 * 1024; // 10MB

    private readonly IIndividualKycService _service;
    private readonly IIndividualKycDocumentService _documentService;

    public IndividualKycController(IIndividualKycService service, IIndividualKycDocumentService documentService)
    {
        _service = service;
        _documentService = documentService;
    }

    [HttpGet("{customerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IndividualKycDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _service.GetActiveAsync(customerId, cancellationToken);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    [HttpPost("{customerId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IndividualKycDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateActive(Guid customerId, [FromBody] UpsertIndividualKycRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateActiveAsync(customerId, dto, cancellationToken);
        if (!result.Success)
            return result.Message == "Customer not found." ? NotFound(result) : BadRequest(result);
        return Ok(result);
    }

    [HttpGet("{customerId:guid}/documents")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<IndividualKycDocumentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDocuments(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _documentService.GetByCustomerIdAsync(customerId, cancellationToken);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("{customerId:guid}/documents")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<IndividualKycDocumentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadDocument(
        Guid customerId,
        IFormFile file,
        [FromForm] string? documentNo,
        [FromForm] DateTime? issuedDate,
        [FromForm] DateTime? expiryDate,
        [FromForm] string? approvedBy,
        [FromForm] string? folderPath,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<IndividualKycDocumentDto>.Fail("No file uploaded."));

        if (file.Length > MaxDocumentSizeBytes)
            return BadRequest(ApiResponse<IndividualKycDocumentDto>.Fail("File size must be less than 10MB."));

        await using var stream = file.OpenReadStream();

        var dto = new UploadIndividualKycDocumentRequestDto
        {
            DocumentNo = documentNo,
            IssuedDate = issuedDate,
            ExpiryDate = expiryDate,
            ApprovedBy = approvedBy,
            FolderPath = folderPath
        };

        var result = await _documentService.UploadAsync(customerId, dto, stream, file.FileName, file.ContentType, cancellationToken);
        if (!result.Success)
            return BadRequest(result);

        return Created($"/api/individual-kyc/{customerId}/documents/{result.Data!.Id}", result);
    }

    [HttpGet("{customerId:guid}/documents/{documentId:guid}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadDocument(Guid customerId, Guid documentId, CancellationToken cancellationToken)
    {
        var result = await _documentService.GetDownloadAsync(customerId, documentId, cancellationToken);
        if (!result.Success)
            return NotFound(result);

        var (content, fileName, contentType) = result.Data!;
        return File(content, contentType, fileName);
    }

    [HttpDelete("{customerId:guid}/documents/{documentId:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDocument(Guid customerId, Guid documentId, CancellationToken cancellationToken)
    {
        var result = await _documentService.DeleteAsync(customerId, documentId, cancellationToken);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }
}

