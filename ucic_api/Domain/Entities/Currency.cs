using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Currency : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CurrencyId { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }

        [Required, MaxLength(3)]
        public string? CurrencyCode { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<Vendor> Vendors { get; set; }
    }
}
