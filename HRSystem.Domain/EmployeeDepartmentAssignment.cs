using System;
using System.Collections.Generic;
using System.Text;

namespace HRSystem.Domain
{
    public class EmployeeDepartmentAssignment
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public int DepartmentId { get; set; }
        public Department Department { get; set; } = null!;

        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }
}
