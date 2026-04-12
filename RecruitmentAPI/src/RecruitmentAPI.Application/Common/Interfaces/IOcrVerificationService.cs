namespace RecruitmentAPI.Application.Common.Interfaces;

public interface IOcrVerificationService
{
    Task VerifyAsync(long candidateDocumentId, CancellationToken cancellationToken = default);
}
