using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class GeneralSettingAndCitiesDTO
    {
        public string VatPercentage { get; set; }
        public string Country { get; set; }

        public IList<CitiesDTO> cities { get; set; }
    }
}
