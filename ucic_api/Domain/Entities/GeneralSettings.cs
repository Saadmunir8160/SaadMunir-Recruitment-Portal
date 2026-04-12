using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class GeneralSettings : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long SettingsID { get; set; }

        [Required, MaxLength(255)]
        public string SettingsGroup { get; set; }

        [Required, MaxLength(255)]
        public string SettingsKey { get; set; }

        [Required, MaxLength(500)]
        public string SettingsValue { get; set; }
    }
}
