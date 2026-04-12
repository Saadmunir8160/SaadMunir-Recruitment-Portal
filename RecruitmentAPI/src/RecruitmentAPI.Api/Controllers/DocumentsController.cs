using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecruitmentAPI.Application.Commands.Documents.ParseCv;
using RecruitmentAPI.Application.Commands.Documents.Upload;
using RecruitmentAPI.Application.Commands.Documents.VerifyOcr;
using RecruitmentAPI.Application.Commands.Documents.VerifyOcr;
using RecruitmentAPI.Application.Common.Interfaces;
using RecruitmentAPI.Application.Queries.Documents;
using RecruitmentAPI.Domain.Enums;

namespace RecruitmentAPI.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IFileStorageService _fileStorage;
    private readonly ICvParsingService _cvParsing;

    public DocumentsController(IMediator mediator, IFileStorageService fileStorage, ICvParsingService cvParsing)
    {
        _mediator = mediator;
        _fileStorage = fileStorage;
        _cvParsing = cvParsing;
    }

    /// <summary>
    /// Upload a document (CV, National ID, Passport, Degree, Certificate, etc.)
    /// for a given candidate.
    /// </summary>
    [HttpPost("candidates/{candidateId}/upload")]
    [Authorize(Roles = RecruitmentPortalRoles.StaffWithCandidate)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Upload(
        long candidateId,
        [FromForm] DocumentType documentType,
        IFormFile file)
    {
        var result = await _mediator.Send(new UploadDocumentCommand
        {
            CandidateId = candidateId,
            DocumentType = documentType,
            File = file
        });

        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// List all documents uploaded by a candidate.
    /// </summary>
    [HttpGet("candidates/{candidateId}")]
    [Authorize(Roles = RecruitmentPortalRoles.StaffWithCandidate)]
    public async Task<IActionResult> GetAllForCandidate(long candidateId)
    {
        var result = await _mediator.Send(new GetCandidateDocumentsQuery { CandidateId = candidateId });
        return Ok(result);
    }

    /// <summary>
    /// Get a single document record by ID.
    /// </summary>
    [HttpGet("{documentId}")]
    [Authorize(Roles = RecruitmentPortalRoles.StaffWithCandidate)]
    public async Task<IActionResult> GetById(long documentId)
    {
        var result = await _mediator.Send(new GetDocumentByIdQuery { DocumentId = documentId });
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Download / stream a document file directly from the server.
    /// </summary>
    [HttpGet("{documentId}/download")]
    [Authorize(Roles = RecruitmentPortalRoles.StaffWithCandidate)]
    public async Task<IActionResult> Download(long documentId)
    {
        var result = await _mediator.Send(new GetDocumentByIdQuery { DocumentId = documentId });
        if (!result.Success || result.Data == null)
            return NotFound(result);

        var absolutePath = _fileStorage.GetAbsolutePath(result.Data.FilePath);
        if (!System.IO.File.Exists(absolutePath))
            return NotFound("File not found on server.");

        var contentType = result.Data.ContentType ?? "application/octet-stream";
        var fileName = result.Data.FileName ?? Path.GetFileName(absolutePath);

        var stream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return File(stream, contentType, fileName);
    }

    /// <summary>
    /// Manually trigger CV parsing for a specific document (re-parse).
    /// Parsing also runs automatically when a CV is uploaded.
    /// </summary>
    [HttpPost("{documentId}/parse-cv")]
    [Authorize(Roles = RecruitmentPortalRoles.StaffWithCandidate)]
    public async Task<IActionResult> ParseCv(long documentId, [FromQuery] long candidateId)
    {
        var result = await _mediator.Send(new ParseCvCommand
        {
            CandidateDocumentId = documentId,
            CandidateId = candidateId
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Parse a CV file and return structured data without saving anything to the database.
    /// Used by the profile creation wizard to auto-fill the form from an uploaded CV.
    /// </summary>
    [HttpPost("parse-cv-preview")]
    [Authorize(Roles = "Candidate")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ParseCvPreview(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { Success = false, Message = "No file provided." });

        if (file.Length > 10 * 1024 * 1024)
            return BadRequest(new { Success = false, Message = "File size exceeds 10 MB." });

        var contentType = file.ContentType?.ToLowerInvariant() ?? string.Empty;
        var allowed = new[] { "application/pdf", "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" };

        if (!Array.Exists(allowed, t => contentType.Contains(t.Split('/')[1])))
            return BadRequest(new { Success = false, Message = "Only PDF and Word documents are supported." });

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, cancellationToken);
        var bytes = ms.ToArray();

        var result = await _cvParsing.ParseCvFromBytesAsync(bytes, file.ContentType!, cancellationToken);

        if (result == null)
            return BadRequest(new { Success = false, Message = "Could not parse the CV. Please try a different file or fill in manually." });

        return Ok(new { Success = true, Data = result });
    }

    /// <summary>
    /// Get OCR verification results for all identity documents uploaded by a candidate.
    /// </summary>
    [HttpGet("candidates/{candidateId}/ocr-results")]
    [Authorize(Roles = RecruitmentPortalRoles.Staff)]
    public async Task<IActionResult> GetOcrResults(long candidateId)
    {
        var result = await _mediator.Send(new GetOcrResultsQuery { CandidateId = candidateId });
        return Ok(result);
    }

    /// <summary>
    /// Manually trigger OCR verification for a specific identity document.
    /// </summary>
    [HttpPost("{documentId}/verify-ocr")]
    [Authorize(Roles = RecruitmentPortalRoles.Staff)]
    public async Task<IActionResult> VerifyOcr(long documentId)
    {
        var result = await _mediator.Send(new VerifyOcrCommand { CandidateDocumentId = documentId });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
