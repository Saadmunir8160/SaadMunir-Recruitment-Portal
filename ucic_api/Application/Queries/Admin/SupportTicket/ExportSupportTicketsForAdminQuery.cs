using MediatR;
using Application.DTOs;

namespace Application.Queries.Admin.SupportTicket
{
    public class ExportSupportTicketsForAdminQuery : IRequest<ExportFileResult>
    {
        public int? DealerId { get; set; }
        public string? Category { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public string? Search { get; set; }
    }

    public class ExportFileResult
    {
        public byte[] FileContent { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
}