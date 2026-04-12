using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class UserWithLocationsDTO
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string CR_No { get; set; }
        public string VAT_ID { get; set; }
        public string ContactPerson { get; set; }
        public IList<LocationDTO> Locations {  get; set; }
    }
}
