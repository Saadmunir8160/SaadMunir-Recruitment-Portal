using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class DealerDailyLimit : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DailyLimitID { get; set; }
        [ForeignKey("Dealer")]
        public int? DealerID { get; set; }
        [MaxLength(50)]
        public string? LimitType { get; set; }
        public decimal? LimitValue { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public virtual Dealer? Dealer { get; set; }
    }
}