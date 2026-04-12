using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class JobApplications : BaseEntity
    {
        public long JobApplicationsId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ResumePath { get; set; }

        [ForeignKey("Order")]
        public long JobsId { get; set; }

        public virtual Jobs Jobs { get; set; }
    }
}
