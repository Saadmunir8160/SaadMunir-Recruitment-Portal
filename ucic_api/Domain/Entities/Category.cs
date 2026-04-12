using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Category : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryId { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(6)]
        public string? CategoryCode { get; set; }
        public string? ArabicName { get; set; }
        public bool Active { get; set; }

        public virtual ICollection<Vendor>? Vendors { get; set; }
    }
}
