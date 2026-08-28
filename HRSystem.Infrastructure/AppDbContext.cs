using HRSystem.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRSystem.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Position> Positions => Set<Position>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<DepartmentHierarchy> DepartmentHierarchies => Set<DepartmentHierarchy>();
        public DbSet<EmployeeDepartmentAssignment> EmployeeDepartmentAssignments => Set<EmployeeDepartmentAssignment>();
        public DbSet<EmployeePositionAssignment> EmployeePositionAssignments => Set<EmployeePositionAssignment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}

