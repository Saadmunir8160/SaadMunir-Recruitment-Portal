using MediatR;
using RecruitmentAPI.Application.Common.Interfaces;
using RecruitmentAPI.Application.DTOs;

namespace RecruitmentAPI.Application.Commands.Documents.ParseCv;

public class ParseCvCommand : IRequest<Response<bool>>
{
    public long CandidateDocumentId { get; set; }
    public long CandidateId { get; set; }
}

public class ParseCvHandler : IRequestHandler<ParseCvCommand, Response<bool>>
{
    private readonly ICvParsingService _cvParsingService;

    public ParseCvHandler(ICvParsingService cvParsingService)
        => _cvParsingService = cvParsingService;

    public async Task<Response<bool>> Handle(ParseCvCommand request, CancellationToken cancellationToken)
    {
        var result = await _cvParsingService.ParseCvAsync(request.CandidateDocumentId, cancellationToken);

        if (result == null)
            return Response<bool>.FailureResponse("CV parsing failed or document not found.");

        await _cvParsingService.AutofillCandidateProfileAsync(request.CandidateId, result, cancellationToken);

        return Response<bool>.SuccessResponse(true, $"CV parsed successfully. Confidence: {result.OverallConfidence}%");
    }
}
