using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRSystem.Domain
{
    public class Position
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;

        public ICollection<EmployeePositionAssignment> Assignments { get; set; } =
            new List<EmployeePositionAssignment>();

    }
}
