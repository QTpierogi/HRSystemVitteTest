using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRSystem.Domain
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public DateTime? DissolvedDate { get; set; }

        public bool IsDissolved => DissolvedDate is not null;

        public ICollection<DepartmentHierarchy> HierarchyRecords { get; set; } =
            new List<DepartmentHierarchy>();

        public ICollection<DepartmentHierarchy> ChildHierarchyRecords { get; set; } =
            new List<DepartmentHierarchy>();

        public ICollection<EmployeeDepartmentAssignment> EmployeeAssignments { get; set; } =
            new List<EmployeeDepartmentAssignment>();
    }
}
