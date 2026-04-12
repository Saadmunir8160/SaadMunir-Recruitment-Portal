namespace Application.Common.Configurations
{
    public class ApiConfiguration
    {
        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }
        public Dictionary<string, string> Endpoints { get; set; }
    }
} 