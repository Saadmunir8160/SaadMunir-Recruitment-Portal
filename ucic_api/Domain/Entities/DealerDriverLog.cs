using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class DealerDriverLog : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogID { get; set; }
        [ForeignKey("DealerOrder")]
        public int DealerOrderID { get; set; }
        [ForeignKey("DealerDriver")]
        public int DriverID { get; set; }
        [MaxLength(100)]
        public string? Action { get; set; }
        public DateTime? Timestamp { get; set; }
        public virtual DealerOrder DealerOrder { get; set; }
        public virtual DealerDriver DealerDriver { get; set; }
    }
}