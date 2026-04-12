using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class DepartmentNotificationRecipient : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; }
        [Required, MaxLength(150)]
        public string EmailAddress { get; set; }
        public string? PhoneNo { get; set; }

        [ForeignKey("Department")]
        public int DepartmentId { get; set; }
        public bool Active { get; set; }
        public virtual Department Department { get; set; }

    }
}

