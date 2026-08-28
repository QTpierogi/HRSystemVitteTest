using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRSystem.Domain
{
    public class Employee
    {
        public int Id { get; set; }
        public string Surname { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Patronim { get; set; }

        public DateTime HireDate { get; set; }
        public DateTime? DismissalDate { get; set; }

        public bool IsActive { get; set; } = false;

        public ICollection<EmployeeDepartmentAssignment> DepartmentAssignments { get; set; } =
        new List<EmployeeDepartmentAssignment>();

        public ICollection<EmployeePositionAssignment> PositionAssignments { get; set; } =
            new List<EmployeePositionAssignment>();

    }
}
