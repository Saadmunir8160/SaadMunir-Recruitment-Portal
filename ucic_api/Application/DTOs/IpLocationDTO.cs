using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class IpLocationDTO
    {
        [JsonPropertyName("query")]
        public string Query { get; set; }       // IP address

        [JsonPropertyName("city")]
        public string City { get; set; }        // City

        [JsonPropertyName("country")]
        public string Country { get; set; }     // Country

        [JsonPropertyName("regionName")]
        public string RegionName { get; set; }  // Region/State

        [JsonPropertyName("lat")]
        public double Latitude { get; set; }    // Latitude

        [JsonPropertyName("lon")]
        public double Longitude { get; set; }   // Longitude
    }
}
