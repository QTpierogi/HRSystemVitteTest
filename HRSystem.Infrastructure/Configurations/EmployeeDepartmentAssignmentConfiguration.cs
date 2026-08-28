using HRSystem.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace HRSystem.Infrastructure.Configurations
{
    internal class EmployeeDepartmentAssignmentConfiguration : IEntityTypeConfiguration<EmployeeDepartmentAssignment>
    {
        public void Configure(EntityTypeBuilder<EmployeeDepartmentAssignment> builder)
        {
            builder.HasOne(x => x.Employee)
                    .WithMany(emp => emp.DepartmentAssignments)
                    .HasForeignKey(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Department)
                .WithMany(d => d.EmployeeAssignments)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.EmployeeId, x.ValidFrom, x.ValidTo });
        }
    }
}
