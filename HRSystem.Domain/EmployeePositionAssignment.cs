using System;
using System.Collections.Generic;
using System.Text;

namespace HRSystem.Domain
{
    public class EmployeePositionAssignment
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public int PositionId { get; set; }
        public Position Position { get; set; } = null!;

        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
    }
}
