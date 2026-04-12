using MediatR;
using Microsoft.AspNetCore.Http;
using RecruitmentAPI.Application.DTOs;
using RecruitmentAPI.Application.DTOs.Documents;
using RecruitmentAPI.Domain.Enums;

namespace RecruitmentAPI.Application.Commands.Documents.Upload;

public class UploadDocumentCommand : IRequest<Response<CandidateDocumentDto>>
{
    public long CandidateId { get; set; }
    public DocumentType DocumentType { get; set; }
    public IFormFile File { get; set; } = null!;
}
