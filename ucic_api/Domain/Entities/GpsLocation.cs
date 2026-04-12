using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class GpsLocation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long GpsLocationId { get; set; }

        [ForeignKey("Order")]
        public long OrderId { get; set; }

        public string? IpAddress { get; set; }       // IP address
        public string? City { get; set; }        // City
        public string? Country { get; set; }     // Country
        public string? RegionName { get; set; }  // Region/State
        public double? Latitude { get; set; }    // Latitude
        public double? Longitude { get; set; }


        public virtual Order Order { get; set; }
    }
}
