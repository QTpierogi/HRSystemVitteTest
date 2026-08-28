using HRSystem.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRSystem.Infrastructure.Seed
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext db)
        {
            if (db.Departments.Any()) return;

            var root = new Department { Name = "Компания" };
            var itDept = new Department { Name = "IT-отдел" };
            db.Departments.AddRange(root, itDept);
            db.SaveChanges();

            db.DepartmentHierarchies.Add(new DepartmentHierarchy
            {
                DepartmentId = root.Id,
                ParentDepartmentId = 0,
                ValidFrom = DateTime.UtcNow.Date,
                ValidTo = null
            });
            db.DepartmentHierarchies.Add(new DepartmentHierarchy
            {
                DepartmentId = itDept.Id,
                ParentDepartmentId = root.Id,
                ValidFrom = DateTime.UtcNow.Date,
                ValidTo = null
            });

            var devPosition = new Position { Title = "Тестовая должность" };
            db.Positions.Add(devPosition);
            db.SaveChanges();

            var employee = new Employee
            {
                Surname = "Иванов",
                Name = "Иван",
                Patronim = "Иванович",
                HireDate = DateTime.UtcNow.Date,
                IsActive = true
            };
            db.Employees.Add(employee);
            db.SaveChanges();

            db.EmployeePositionAssignments.Add(new EmployeePositionAssignment
            {
                EmployeeId = employee.Id,
                PositionId = devPosition.Id,
                ValidFrom = DateTime.UtcNow.Date,
                ValidTo = null
            });

            db.EmployeeDepartmentAssignments.Add(new EmployeeDepartmentAssignment
            {
                EmployeeId = employee.Id,
                DepartmentId = itDept.Id,
                ValidFrom = DateTime.UtcNow.Date,
                ValidTo = null
            });

            db.SaveChanges();
        }
    }
}
