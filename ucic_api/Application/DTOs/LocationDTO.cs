using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class LocationDTO
    {
        public long LocationID { get; set; }

        [Required(ErrorMessage = "CustomerId is required")]
        public long CustomerId { get; set; }

        [Required(ErrorMessage = "CoverageAreaId is required")]
        public long CoverageAreaId { get; set; }

        [Required(ErrorMessage = "CityID is required")]
        public long CityId { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string? Address { get; set; }

        public string? ZipCode { get; set; }

        [Required(ErrorMessage = "GpsCoordinates is required")]
        public string? GpsCoordinates { get; set; }
    }
}
