using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Department : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DepartmentId { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; }
        
        public bool Active { get; set; }
        
        public bool IsActive { get; set; }
        
        public bool IsDeleted { get; set; }
        
        public DateTime CreatedDate { get; set; }
        
        [MaxLength(100)]
        public string? CreatedBy { get; set; }
        
        public DateTime? ModifiedDate { get; set; }
        
        [MaxLength(100)]
        public string? ModifiedBy { get; set; }

        public virtual ICollection<DepartmentNotificationRecipient> DepartmentNotificationRecipients { get; set; }
    }
}