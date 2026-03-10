using AmlScreening.Application.Common;
using AmlScreening.Application.DTOs.Customers;
using AmlScreening.Application.DTOs.SanctionsScreening;
using AmlScreening.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmlScreening.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class CustomersController : ControllerBase
{
    private const long MaxDocumentSizeBytes = 10 * 1024 * 1024; // 10MB

    private readonly ICustomerService _service;
    private readonly ICustomerDocumentService _documentService;
    private readonly IRunSanctionsScreeningService _runSanctionsScreeningService;
    private readonly ISanctionActionAuditLogService _sanctionActionAuditLogService;

    public CustomersController(ICustomerService service, ICustomerDocumentService documentService, IRunSanctionsScreeningService runSanctionsScreeningService, ISanctionActionAuditLogService sanctionActionAuditLogService)
    {
        _service = service;
        _documentService = documentService;
        _runSanctionsScreeningService = runSanctionsScreeningService;
        _sanctionActionAuditLogService = sanctionActionAuditLogService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CustomerDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaged([FromQuery] PagedRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.GetPagedAsync(request, cancellationToken);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        if (!result.Success)
            return BadRequest(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, dto, cancellationToken);
        if (!result.Success)
            return result.Message == "Customer not found." ? NotFound(result) : BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(id, cancellationToken);
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    [HttpPost("{customerId:guid}/documents")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDocumentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadDocument(
        Guid customerId,
        IFormFile file,
        [FromForm] string documentTypeCode,
        [FromForm] DateTime? expiryDate,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<CustomerDocumentDto>.Fail("No file uploaded."));
        if (file.Length > MaxDocumentSizeBytes)
            return BadRequest(ApiResponse<CustomerDocumentDto>.Fail("File size must be less than 10MB."));
        if (string.IsNullOrWhiteSpace(documentTypeCode))
            return BadRequest(ApiResponse<CustomerDocumentDto>.Fail("Document type is required."));

        await using var stream = file.OpenReadStream();
        var result = await _documentService.UploadAsync(customerId, documentTypeCode.Trim(), stream, file.FileName, file.ContentType, expiryDate, cancellationToken);
        if (!result.Success)
            return result.Message == "Customer not found." ? NotFound(result) : BadRequest(result);
        return Created($"/api/Customers/{customerId}/documents/{result.Data!.Id}", result);
    }

    [HttpGet("{customerId:guid}/documents")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CustomerDocumentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerDocuments(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _documentService.GetByCustomerIdAsync(customerId, cancellationToken);
        if (!result.Success)
            return Ok(result);
        return Ok(result);
    }

    [HttpGet("{customerId:guid}/documents/{documentId:guid}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadDocument(Guid customerId, Guid documentId, CancellationToken cancellationToken)
    {
        var result = await _documentService.GetDownloadAsync(customerId, documentId, cancellationToken);
        if (!result.Success)
            return NotFound(ApiResponse.Fail(result.Message));
        var (content, fileName, contentType) = result.Data!;
        return File(content, contentType, fileName);
    }

    [HttpPost("{customerId:guid}/sanctions-screening/run")]
    [ProducesResponseType(typeof(ApiResponse<RunSanctionsScreeningResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RunSanctionsScreening(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _runSanctionsScreeningService.RunScreeningForCustomerAsync(customerId, cancellationToken);
        if (!result.Success)
            return result.Message == "Customer not found." ? NotFound(result) : BadRequest(result);
        return Ok(result);
    }

    [HttpGet("{customerId:guid}/sanctions-screening/results")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SanctionsScreeningResultItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSanctionsScreeningResults(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await _runSanctionsScreeningService.GetResultsForCustomerAsync(customerId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{customerId:guid}/sanctions-screening/{screeningId:guid}/action")]
    [ProducesResponseType(typeof(ApiResponse<SanctionsScreeningResultItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordSanctionScreeningAction(Guid customerId, Guid screeningId, [FromBody] RecordSanctionScreeningActionRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            return BadRequest(ApiResponse<SanctionsScreeningResultItemDto>.Fail("Request body is required."));
        var result = await _sanctionActionAuditLogService.RecordCheckerActionAsync(customerId, screeningId, request, cancellationToken);
        if (!result.Success)
            return result.Message == "Screening result not found." ? NotFound(result) : BadRequest(result);
        return Ok(result);
    }
}
