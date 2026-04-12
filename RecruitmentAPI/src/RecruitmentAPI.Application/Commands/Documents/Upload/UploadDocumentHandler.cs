using MediatR;
using Microsoft.AspNetCore.Http;
using RecruitmentAPI.Application.Commands.Documents.ParseCv;
using RecruitmentAPI.Application.Commands.Documents.VerifyOcr;
using RecruitmentAPI.Application.Common.Interfaces;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Application.DTOs.Documents;
using RecruitmentAPI.Domain.Entities;
using RecruitmentAPI.Domain.Enums;
using RecruitmentAPI.Domain.Repositories.Command.Base;
using RecruitmentAPI.Domain.Repositories.Query.Base;
using System.Security.Claims;

namespace RecruitmentAPI.Application.Commands.Documents.Upload;

public class UploadDocumentHandler : IRequestHandler<UploadDocumentCommand, Response<CandidateDocumentDto>>
{
    private readonly ICommandRepository<CandidateDocument> _commandRepository;
    private readonly IQueryRepository<Candidate> _candidateQuery;
    private readonly IFileStorageService _fileStorage;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMediator _mediator;

    // Allowed MIME types per document category
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "image/jpeg",
        "image/png",
        "image/jpg"
    };

    // 10 MB
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    public UploadDocumentHandler(
        ICommandRepository<CandidateDocument> commandRepository,
        IQueryRepository<Candidate> candidateQuery,
        IFileStorageService fileStorage,
        IHttpContextAccessor httpContextAccessor,
        IMediator mediator)
    {
        _commandRepository = commandRepository;
        _candidateQuery = candidateQuery;
        _fileStorage = fileStorage;
        _httpContextAccessor = httpContextAccessor;
        _mediator = mediator;
    }

    public async Task<Response<CandidateDocumentDto>> Handle(UploadDocumentCommand request, CancellationToken cancellationToken)
    {
        var userId = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Response<CandidateDocumentDto>.FailureResponse("User not authenticated.");

        // Validate candidate exists
        var candidate = _candidateQuery.GetQueryable()
            .FirstOrDefault(c => c.CandidateId == request.CandidateId && !c.IsDeleted);

        if (candidate == null)
            return Response<CandidateDocumentDto>.FailureResponse("Candidate not found.");

        // Validate file
        var file = request.File;
        if (file == null || file.Length == 0)
            return Response<CandidateDocumentDto>.FailureResponse("No file provided.");

        if (file.Length > MaxFileSizeBytes)
            return Response<CandidateDocumentDto>.FailureResponse("File size exceeds the 10 MB limit.");

        if (!AllowedContentTypes.Contains(file.ContentType))
            return Response<CandidateDocumentDto>.FailureResponse(
                "Invalid file type. Allowed: PDF, Word (doc/docx), JPEG, PNG.");

        // Save file to disk
        var subFolder = $"candidates/{request.CandidateId}/{request.DocumentType}";
        string relativePath;
        await using (var stream = file.OpenReadStream())
        {
            relativePath = await _fileStorage.SaveFileAsync(stream, file.FileName, subFolder, cancellationToken);
        }

        var modifierName = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

        var document = new CandidateDocument
        {
            CandidateId = request.CandidateId,
            DocumentType = request.DocumentType,
            FileName = file.FileName,
            FilePath = relativePath,
            FileSize = file.Length,
            ContentType = file.ContentType,
            AiProcessingStatus = AiProcessingStatus.Pending,
            UploadedDate = DateTime.UtcNow,
            IsActive = true,
            CreatedBy = modifierName
        };

        await _commandRepository.AddAsync(document);

        // If this is a CV, automatically kick off background parsing (fire-and-forget)
        if (request.DocumentType == DocumentType.CV)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _mediator.Send(new ParseCvCommand
                    {
                        CandidateDocumentId = document.CandidateDocumentId,
                        CandidateId = document.CandidateId
                    });
                }
                catch
                {
                    // Parsing failure is non-fatal; status is tracked in CandidateDocuments
                }
            }, CancellationToken.None);
        }

        // If this is a National ID or Passport, trigger OCR verification (fire-and-forget)
        if (request.DocumentType == DocumentType.NationalId || request.DocumentType == DocumentType.Passport)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await _mediator.Send(new VerifyOcrCommand
                    {
                        CandidateDocumentId = document.CandidateDocumentId
                    });
                }
                catch
                {
                    // OCR failure is non-fatal; status is tracked in CandidateDocuments
                }
            }, CancellationToken.None);
        }

        var dto = new CandidateDocumentDto
        {
            CandidateDocumentId = document.CandidateDocumentId,
            CandidateId = document.CandidateId,
            DocumentType = document.DocumentType,
            FileName = document.FileName,
            FilePath = document.FilePath,
            FileSize = document.FileSize,
            ContentType = document.ContentType,
            AiProcessingStatus = document.AiProcessingStatus,
            UploadedDate = document.UploadedDate
        };

        return Response<CandidateDocumentDto>.SuccessResponse(dto, "Document uploaded successfully.");
    }
}
