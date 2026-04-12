using Application.Common.Configurations;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Application.Common.Services
{
    public interface IExternalApiService
    {
        Task<T> PostAsync<T>(string endpoint, object data);
        Task<T> GetAsync<T>(string endpoint);
        Task<T> GetAsync<T>(string endpoint, Dictionary<string, string> queryParameters);
    }

    public class ExternalApiService : IExternalApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ApiConfiguration _apiConfig;
        private readonly ILogger<ExternalApiService> _logger;

        public ExternalApiService(
            HttpClient httpClient, 
            IOptions<ApiConfiguration> apiConfig,
            ILogger<ExternalApiService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiConfig = apiConfig?.Value ?? throw new ArgumentNullException(nameof(apiConfig));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            if (string.IsNullOrEmpty(_apiConfig.ApiKey))
                throw new ArgumentException("API Key is not configured", nameof(apiConfig));
                
            _httpClient.DefaultRequestHeaders.Add("apikey", _apiConfig.ApiKey);
        }

        public async Task<T> PostAsync<T>(string endpoint, object data)
        {
            if (string.IsNullOrEmpty(endpoint))
                throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));
                
            if (data == null)
                throw new ArgumentNullException(nameof(data));
                
            if (_apiConfig.Endpoints == null || !_apiConfig.Endpoints.ContainsKey(endpoint))
                throw new KeyNotFoundException($"Endpoint '{endpoint}' not found in configuration");

            var fullUrl = $"{_apiConfig.BaseUrl}{_apiConfig.Endpoints[endpoint]}";
            var jsonContent = JsonSerializer.Serialize(data);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _logger.LogInformation($"Making POST request to {fullUrl}");
            _logger.LogDebug($"Request body: {jsonContent}");

            try
            {
                var response = await _httpClient.PostAsync(fullUrl, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"API request failed. Status: {response.StatusCode}. Response: {errorContent}");
                    throw new HttpRequestException($"API request failed with status {response.StatusCode}. Response: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Response content: {responseContent}");
                
                return JsonSerializer.Deserialize<T>(responseContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error making POST request to {fullUrl}");
                throw;
            }
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            return await GetAsync<T>(endpoint, null);
        }

        public async Task<T> GetAsync<T>(string endpoint, Dictionary<string, string>? queryParameters)
        {
            if (string.IsNullOrEmpty(endpoint))
                throw new ArgumentException("Endpoint cannot be null or empty", nameof(endpoint));
                
            if (_apiConfig.Endpoints == null || !_apiConfig.Endpoints.ContainsKey(endpoint))
                throw new KeyNotFoundException($"Endpoint '{endpoint}' not found in configuration");

            var baseEndpoint = _apiConfig.Endpoints[endpoint];
            var fullUrl = $"{_apiConfig.BaseUrl}{baseEndpoint}";
            
            // Append query parameters if provided
            if (queryParameters != null && queryParameters.Count > 0)
            {
                var queryString = string.Join("&", queryParameters.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
                fullUrl += $"?{queryString}";
            }
            
            _logger.LogInformation($"Making GET request to {fullUrl}");

            try
            {
                var response = await _httpClient.GetAsync(fullUrl);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"API request failed. Status: {response.StatusCode}. Response: {errorContent}");
                    throw new HttpRequestException($"API request failed with status {response.StatusCode}. Response: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogDebug($"Response content: {responseContent}");
                
                return JsonSerializer.Deserialize<T>(responseContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error making GET request to {fullUrl}");
                throw;
            }
        }
    }
} 