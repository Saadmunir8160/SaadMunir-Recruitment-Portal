using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Location : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long LocationId { get; set; }

        [ForeignKey("Customer")]
        public long CustomerId { get; set; }

        [ForeignKey("CoverageArea")]
        public long CoverageAreaId { get; set; }

        [ForeignKey("Cities")]
        public long CitiesId { get; set; }

        [Required, MaxLength(255)]
        public string? Address { get; set; }

        //[Required, MaxLength(100)]
        //public string? City { get; set; }

        //[Required, MaxLength(100)]
        //public string? Country { get; set; }

        [MaxLength(20)]
        public string? ZipCode { get; set; }

        [MaxLength(100)]
        public string? GpsCoordinates { get; set; }

        // Navigation property
        public virtual CoverageArea CoverageArea { get; set; }
        public virtual Cities Cities { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }
}
