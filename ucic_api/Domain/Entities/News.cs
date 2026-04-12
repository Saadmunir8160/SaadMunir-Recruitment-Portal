using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class News : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long NewsId { get; set; }

        [Required, MaxLength(255)]
        public string? Title { get; set; }

        [Required]
        public string? Content { get; set; }

        public string? ImageUrl { get; set; }
        public bool IsArabic { get; set; }

        public NewsType Type { get; set; } = NewsType.Internal;
    }

    public enum NewsType
    {
        Internal,
        Twitter
    }
}
