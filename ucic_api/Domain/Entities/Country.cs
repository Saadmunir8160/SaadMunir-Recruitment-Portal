using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Country : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CountryId { get; set; }

        [Required, MaxLength(50)]
        public string CountryName { get; set; }

        [Required, MaxLength(3)]
        public string? CountryCode { get; set; }
        public virtual ICollection<Vendor> Vendors { get; set; }
        public virtual ICollection<CitiesByCountry> CitiesByCountry { get; set; }
    }
}
