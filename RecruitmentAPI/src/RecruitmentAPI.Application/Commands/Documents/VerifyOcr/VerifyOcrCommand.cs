using MediatR;
using RecruitmentAPI.Application.Common.Interfaces;
using RecruitmentAPI.Application.DTOs;

namespace RecruitmentAPI.Application.Commands.Documents.VerifyOcr;

public class VerifyOcrCommand : IRequest<Response<bool>>
{
    public long CandidateDocumentId { get; set; }
}

public class VerifyOcrHandler : IRequestHandler<VerifyOcrCommand, Response<bool>>
{
    private readonly IOcrVerificationService _ocrService;

    public VerifyOcrHandler(IOcrVerificationService ocrService)
        => _ocrService = ocrService;

    public async Task<Response<bool>> Handle(VerifyOcrCommand request, CancellationToken cancellationToken)
    {
        await _ocrService.VerifyAsync(request.CandidateDocumentId, cancellationToken);
        return Response<bool>.SuccessResponse(true, "OCR verification completed.");
    }
}
