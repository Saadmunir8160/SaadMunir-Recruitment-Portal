using Application.Common.Interfaces;
using Application.DTOs;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Services
{
    public class IpLocationService : IIpLocationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiIpUrl;

        public IpLocationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiIpUrl = configuration["ApiSettings:IpLocationUrl"];
        }

        // Method to fetch location data based on IP address
        public async Task<IpLocationDTO> GetLocationAsync(string ipAddress)
        {
            try
            {
                var url = $"{_apiIpUrl}{ipAddress}";
                var response = await _httpClient.GetStringAsync(url);

                //var options = new System.Text.Json.JsonSerializerOptions
                //{
                //    PropertyNameCaseInsensitive = true
                //};

                // If the response is successful, deserialize it
                return System.Text.Json.JsonSerializer.Deserialize<IpLocationDTO>(response/*, options*/);
            }
            catch (Exception ex)
            {
                // Handle or log the exception as necessary
                //throw new Exception("Error fetching IP location", ex);
                return new IpLocationDTO();
            }
        }
    }
}
