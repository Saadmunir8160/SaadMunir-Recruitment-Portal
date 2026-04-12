using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class JobsDTO
    {
        public long JobsId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public long DepartmentId { get; set; }
        public DateTime PostedDate { get; set; }
        public string WorkType { get; set; }
        public string WorkLocation { get; set; }
        public long Salary { get; set; }
        public bool IsArabic { get; set; }
    }
}
