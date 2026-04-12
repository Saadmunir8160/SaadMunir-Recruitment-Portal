using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Cities : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long CitiesId { get; set; }

        [ForeignKey("CoverageArea")]
        public long CoverageAreaId { get; set; }

        [Required, MaxLength(150)]
        public string CityName { get; set; }

        public virtual CoverageArea CoverageArea { get; set; }
        public virtual ICollection<Location> Locations { get; set; }
    }
}
