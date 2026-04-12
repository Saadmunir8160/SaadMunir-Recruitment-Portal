using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Jobs : BaseEntity
    {
        public long JobsId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Department { get; set; }
        public DateTime PostedDate { get; set; } = DateTime.UtcNow;
        public WorkType WorkType { get; set; } = WorkType.FullTime;
        public WorkLocation WorkLocation { get; set; } = WorkLocation.OnSite;
        public long salary { get; set; }
        [ForeignKey("Department")]
        public long DepartmentId { get; set; }
        public bool IsArabic { get; set; }

        public virtual ICollection<Department> Departments { get; set; }
        public virtual ICollection<JobApplications> JobApplications { get; set; }
    }

    public enum WorkType
    {
        FullTime,
        PartTime
    }

    public enum WorkLocation
    {
        Remote,
        OnSite
    }
}
