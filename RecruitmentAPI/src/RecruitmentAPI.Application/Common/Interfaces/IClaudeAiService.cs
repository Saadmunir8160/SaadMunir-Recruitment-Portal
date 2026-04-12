namespace RecruitmentAPI.Application.Common.Interfaces;

/// <summary>
/// Core wrapper around the Claude Anthropic API.
/// All other AI services (CV parsing, OCR, matching) delegate to this.
/// </summary>
public interface IClaudeAiService
{
    /// <summary>
    /// Send a text-only message to Claude and get the raw text response.
    /// </summary>
    Task<string> SendMessageAsync(string systemPrompt, string userMessage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a message with a base64-encoded image (for OCR / ID document verification).
    /// </summary>
    Task<string> SendMessageWithImageAsync(string systemPrompt, string userMessage, byte[] imageBytes, string mediaType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a message with a base64-encoded PDF document (Claude reads PDFs natively).
    /// </summary>
    Task<string> SendMessageWithPdfAsync(string systemPrompt, string userMessage, byte[] pdfBytes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a text message and deserialize the JSON response into T.
    /// </summary>
    Task<T?> SendStructuredMessageAsync<T>(string systemPrompt, string userMessage, CancellationToken cancellationToken = default) where T : class;
}
