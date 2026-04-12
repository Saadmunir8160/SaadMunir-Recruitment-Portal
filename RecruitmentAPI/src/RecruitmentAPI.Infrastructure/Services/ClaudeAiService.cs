using Microsoft.Extensions.Configuration;
using RecruitmentAPI.Application.Common.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace RecruitmentAPI.Infrastructure.Services;

public class ClaudeAiService : IClaudeAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _model;
    private readonly int _maxTokens;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ClaudeAiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _model = configuration["ClaudeAi:Model"] ?? "claude-sonnet-4-20250514";
        _maxTokens = configuration.GetValue<int>("ClaudeAi:MaxTokens", 4096);

        var apiKey = configuration["ClaudeAi:ApiKey"]
            ?? throw new InvalidOperationException("ClaudeAi:ApiKey is not configured.");

        _httpClient.BaseAddress = new Uri(
            configuration["ClaudeAi:BaseUrl"] ?? "https://api.anthropic.com/v1/messages");

        _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        var timeoutSeconds = configuration.GetValue<int>("ClaudeAi:TimeoutSeconds", 60);
        _httpClient.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
    }

    public async Task<string> SendMessageAsync(string systemPrompt, string userMessage, CancellationToken cancellationToken = default)
    {
        var body = BuildBody(systemPrompt, new[]
        {
            new JsonObject { ["type"] = "text", ["text"] = userMessage }
        });

        return await PostAsync(body, cancellationToken);
    }

    public async Task<string> SendMessageWithImageAsync(string systemPrompt, string userMessage, byte[] imageBytes, string mediaType, CancellationToken cancellationToken = default)
    {
        var base64 = Convert.ToBase64String(imageBytes);

        var body = BuildBody(systemPrompt, new[]
        {
            new JsonObject
            {
                ["type"] = "image",
                ["source"] = new JsonObject
                {
                    ["type"] = "base64",
                    ["media_type"] = mediaType,
                    ["data"] = base64
                }
            },
            new JsonObject { ["type"] = "text", ["text"] = userMessage }
        });

        return await PostAsync(body, cancellationToken);
    }

    public async Task<string> SendMessageWithPdfAsync(string systemPrompt, string userMessage, byte[] pdfBytes, CancellationToken cancellationToken = default)
    {
        var base64 = Convert.ToBase64String(pdfBytes);

        var body = BuildBody(systemPrompt, new[]
        {
            new JsonObject
            {
                ["type"] = "document",
                ["source"] = new JsonObject
                {
                    ["type"] = "base64",
                    ["media_type"] = "application/pdf",
                    ["data"] = base64
                }
            },
            new JsonObject { ["type"] = "text", ["text"] = userMessage }
        });

        return await PostAsync(body, cancellationToken);
    }

    public async Task<T?> SendStructuredMessageAsync<T>(string systemPrompt, string userMessage, CancellationToken cancellationToken = default) where T : class
    {
        var rawResponse = await SendMessageAsync(systemPrompt, userMessage, cancellationToken);

        // Extract JSON block from the response (Claude may wrap it in markdown ```json ... ```)
        var json = ExtractJson(rawResponse);
        return string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<T>(json, JsonOpts);
    }

    // ── Private helpers ─────────────────────────────────────────────────────────

    private JsonObject BuildBody(string systemPrompt, JsonObject[] contentParts)
    {
        var contentArray = new JsonArray();
        foreach (var part in contentParts)
            contentArray.Add(part);

        return new JsonObject
        {
            ["model"] = _model,
            ["max_tokens"] = _maxTokens,
            ["system"] = systemPrompt,
            ["messages"] = new JsonArray
            {
                new JsonObject
                {
                    ["role"] = "user",
                    ["content"] = contentArray
                }
            }
        };
    }

    private async Task<string> PostAsync(JsonObject body, CancellationToken cancellationToken)
    {
        const int maxRetries = 3;
        var delays = new[] { TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(30) };

        for (var attempt = 0; attempt <= maxRetries; attempt++)
        {
            var json = body.ToJsonString();
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            // BaseAddress is already set to the full messages URL
            using var response = await _httpClient.PostAsync(string.Empty, content, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            // 529 = Anthropic overloaded; 429 = too many requests — retry with backoff
            if (((int)response.StatusCode == 529 || (int)response.StatusCode == 429) && attempt < maxRetries)
            {
                await Task.Delay(delays[attempt], cancellationToken);
                continue;
            }

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException(
                    $"Claude API error {(int)response.StatusCode}: {responseBody}");

            // Parse: response.content[0].text
            var doc = JsonDocument.Parse(responseBody);
            var text = doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString();

            return text ?? string.Empty;
        }

        throw new HttpRequestException("Claude API is overloaded. Please try again later.");
    }

    private static string ExtractJson(string text)
    {
        // Strip markdown code fences if present
        var start = text.IndexOf("```json", StringComparison.OrdinalIgnoreCase);
        if (start >= 0)
        {
            start = text.IndexOf('\n', start) + 1;
            var end = text.IndexOf("```", start, StringComparison.OrdinalIgnoreCase);
            if (end > start)
                return text[start..end].Trim();
        }

        // Try to find raw JSON object
        var braceStart = text.IndexOf('{');
        var braceEnd = text.LastIndexOf('}');
        if (braceStart >= 0 && braceEnd > braceStart)
            return text[braceStart..(braceEnd + 1)];

        return text.Trim();
    }
}
