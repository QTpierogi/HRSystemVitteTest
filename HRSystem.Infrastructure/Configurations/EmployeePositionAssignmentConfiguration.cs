using HRSystem.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRSystem.Infrastructure.Configurations
{
    internal class EmployeePositionAssignmentConfiguration : IEntityTypeConfiguration<EmployeePositionAssignment>
    {
        public void Configure(EntityTypeBuilder<EmployeePositionAssignment> builder)
        {
            builder.HasOne(x => x.Employee)
                    .WithMany(emp => emp.PositionAssignments)
                    .HasForeignKey(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Position)
                .WithMany(p => p.Assignments)
                .HasForeignKey(x => x.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.EmployeeId, x.ValidFrom, x.ValidTo });
        }
    }
}
