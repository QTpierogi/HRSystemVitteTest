using HRSystem.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRSystem.Infrastructure.Configurations
{
    internal class DepartmentHierarchyConfiguration : IEntityTypeConfiguration<DepartmentHierarchy>
    {
        public void Configure(EntityTypeBuilder<DepartmentHierarchy> builder)
        {
            builder.HasKey(h => h.Id);

            builder.HasOne(h => h.Department)
                .WithMany(d => d.HierarchyRecords)
                .HasForeignKey(h => h.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict); 

            builder.HasOne(h => h.ParentDepartment)
                .WithMany()
                .HasForeignKey(h => h.ParentDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(h => new { h.DepartmentId, h.ValidFrom, h.ValidTo });
        }
    }
}
