using MediatR;
using Microsoft.EntityFrameworkCore;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Application.DTOs.Documents;
using RecruitmentAPI.Domain.Entities;
using RecruitmentAPI.Domain.Repositories.Query.Base;

namespace RecruitmentAPI.Application.Queries.Documents;

// ─── Get all documents for a candidate ──────────────────────────────────────
public class GetCandidateDocumentsQuery : IRequest<Response<List<CandidateDocumentDto>>>
{
    public long CandidateId { get; set; }
}

public class GetCandidateDocumentsHandler : IRequestHandler<GetCandidateDocumentsQuery, Response<List<CandidateDocumentDto>>>
{
    private readonly IQueryRepository<CandidateDocument> _repository;

    public GetCandidateDocumentsHandler(IQueryRepository<CandidateDocument> repository)
        => _repository = repository;

    public async Task<Response<List<CandidateDocumentDto>>> Handle(GetCandidateDocumentsQuery request, CancellationToken cancellationToken)
    {
        var documents = await _repository.GetQueryable()
            .Where(d => d.CandidateId == request.CandidateId && !d.IsDeleted)
            .OrderByDescending(d => d.UploadedDate)
            .Select(d => new CandidateDocumentDto
            {
                CandidateDocumentId = d.CandidateDocumentId,
                CandidateId = d.CandidateId,
                DocumentType = d.DocumentType,
                FileName = d.FileName,
                FilePath = d.FilePath,
                FileSize = d.FileSize,
                ContentType = d.ContentType,
                AiProcessingStatus = d.AiProcessingStatus,
                UploadedDate = d.UploadedDate
            })
            .ToListAsync(cancellationToken);

        return Response<List<CandidateDocumentDto>>.SuccessResponse(documents);
    }
}

// ─── Get a single document by ID ─────────────────────────────────────────────
public class GetDocumentByIdQuery : IRequest<Response<CandidateDocumentDto>>
{
    public long DocumentId { get; set; }
}

public class GetDocumentByIdHandler : IRequestHandler<GetDocumentByIdQuery, Response<CandidateDocumentDto>>
{
    private readonly IQueryRepository<CandidateDocument> _repository;

    public GetDocumentByIdHandler(IQueryRepository<CandidateDocument> repository)
        => _repository = repository;

    public async Task<Response<CandidateDocumentDto>> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        var d = await _repository.GetQueryable()
            .FirstOrDefaultAsync(d => d.CandidateDocumentId == request.DocumentId && !d.IsDeleted, cancellationToken);

        if (d == null)
            return Response<CandidateDocumentDto>.FailureResponse("Document not found.");

        return Response<CandidateDocumentDto>.SuccessResponse(new CandidateDocumentDto
        {
            CandidateDocumentId = d.CandidateDocumentId,
            CandidateId = d.CandidateId,
            DocumentType = d.DocumentType,
            FileName = d.FileName,
            FilePath = d.FilePath,
            FileSize = d.FileSize,
            ContentType = d.ContentType,
            AiProcessingStatus = d.AiProcessingStatus,
            UploadedDate = d.UploadedDate
        });
    }
}

// ─── Get OCR verification results for a candidate ───────────────────────────
public class GetOcrResultsQuery : IRequest<Response<List<OcrVerificationResultDto>>>
{
    public long CandidateId { get; set; }
}

public class GetOcrResultsHandler : IRequestHandler<GetOcrResultsQuery, Response<List<OcrVerificationResultDto>>>
{
    private readonly IQueryRepository<OcrVerificationResult> _repository;

    public GetOcrResultsHandler(IQueryRepository<OcrVerificationResult> repository)
        => _repository = repository;

    public async Task<Response<List<OcrVerificationResultDto>>> Handle(GetOcrResultsQuery request, CancellationToken cancellationToken)
    {
        var results = await _repository.GetQueryable()
            .Where(r => r.CandidateId == request.CandidateId && !r.IsDeleted)
            .OrderByDescending(r => r.ProcessedDate)
            .ThenBy(r => r.CandidateDocumentId)
            .Select(r => new OcrVerificationResultDto
            {
                OcrVerificationId = r.OcrVerificationId,
                CandidateDocumentId = r.CandidateDocumentId,
                CandidateId = r.CandidateId,
                FieldName = r.FieldName,
                ExtractedValue = r.ExtractedValue,
                EnteredValue = r.EnteredValue,
                IsMatch = r.IsMatch,
                ConfidenceScore = r.ConfidenceScore,
                MismatchSeverity = r.MismatchSeverity,
                ProcessedDate = r.ProcessedDate
            })
            .ToListAsync(cancellationToken);

        return Response<List<OcrVerificationResultDto>>.SuccessResponse(results);
    }
}
