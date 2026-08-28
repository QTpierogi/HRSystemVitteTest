using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRSystem.Domain
{
    public class DepartmentHierarchy
    {
        public int Id { get; set; }

        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        public int? ParentDepartmentId { get; set; }
        public Department? ParentDepartment { get; set; }

        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; } 

    }
}
