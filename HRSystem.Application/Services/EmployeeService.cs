using HRSystem.Application;
using HRSystem.Application.DTO;
using HRSystem.Application.Interfaces;
using HRSystem.Domain;
using HRSystem.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRSystem.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(AppDbContext db, ILogger<EmployeeService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Result<EmployeeResponse>> HireAsync(HireEmployeeRequest request, CancellationToken ct = default)
        {
            var department = await _db.Departments.FindAsync(new object?[] { request.DepartmentId }, ct);
            if (department is null)
                return Result<EmployeeResponse>.NotFound($"Подразделение с id={request.DepartmentId} не найдено.");

            var position = await _db.Positions.FindAsync(new object?[] { request.PositionId }, ct);
            if (position is null)
                return Result<EmployeeResponse>.NotFound($"Должность с id={request.PositionId} не найдена.");

            var employee = new Employee
            {
                Surname = request.Surname,
                Name = request.Name,
                Patronim = request.Patronim,
                HireDate = request.HireDate.Date
            };
            _db.Employees.Add(employee);
            await _db.SaveChangesAsync(ct);

            _db.EmployeeDepartmentAssignments.Add(new EmployeeDepartmentAssignment
            {
                EmployeeId = employee.Id,
                DepartmentId = request.DepartmentId,
                ValidFrom = request.HireDate.Date,
                ValidTo = null
            });
            _db.EmployeePositionAssignments.Add(new EmployeePositionAssignment
            {
                EmployeeId = employee.Id,
                PositionId = request.PositionId,
                ValidFrom = request.HireDate.Date,
                ValidTo = null
            });

            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Принят сотрудник {Id} '{Surname}' '{Name}' '{Patronim}' в подразделение {DepartmentId} на должность {PositionId}",
                employee.Id, employee.Surname, employee.Name, employee.Patronim, request.DepartmentId, request.PositionId);

            return await GetByIdAsync(employee.Id, ct);
        }

        public async Task<Result<EmployeeResponse>> DismissAsync(int employeeId, DismissEmployeeRequest request, CancellationToken ct = default)
        {
            var employee = await _db.Employees.FindAsync(new object?[] { employeeId }, ct);
            if (employee is null)
                return Result<EmployeeResponse>.NotFound($"Сотрудник с id={employeeId} не найден.");

            if (!employee.IsActive)
                return Result<EmployeeResponse>.Conflict("Сотрудник уже уволен.");

            if (request.DismissalDate.Date < employee.HireDate.Date)
                return Result<EmployeeResponse>.Failure("Дата увольнения не может быть раньше даты приёма на работу.");

            employee.DismissalDate = request.DismissalDate.Date;

            var openDept = await _db.EmployeeDepartmentAssignments
                .Where(a => a.EmployeeId == employeeId && a.ValidTo == null)
                .FirstOrDefaultAsync(ct);
            if (openDept is not null) openDept.ValidTo = request.DismissalDate.Date;

            var openPos = await _db.EmployeePositionAssignments
                .Where(a => a.EmployeeId == employeeId && a.ValidTo == null)
                .FirstOrDefaultAsync(ct);
            if (openPos is not null) openPos.ValidTo = request.DismissalDate.Date;

            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Уволен сотрудник {Id} с {Date}", employeeId, request.DismissalDate.Date);

            return await GetByIdAsync(employeeId, ct);
        }

        public async Task<Result<EmployeeResponse>> TransferDepartmentAsync(
            int employeeId, TransferDepartmentRequest request, CancellationToken ct = default)
        {
            var employee = await _db.Employees.FindAsync(new object?[] { employeeId }, ct);
            if (employee is null)
                return Result<EmployeeResponse>.NotFound($"Сотрудник с id={employeeId} не найден.");

            if (!employee.IsActive)
                return Result<EmployeeResponse>.Conflict("Нельзя перевести уволенного сотрудника.");

            var newDepartment = await _db.Departments.FindAsync(new object?[] { request.NewDepartmentId }, ct);
            if (newDepartment is null)
                return Result<EmployeeResponse>.NotFound($"Подразделение с id={request.NewDepartmentId} не найдено.");

            var effectiveDate = request.EffectiveDate.Date;
            if (effectiveDate < employee.HireDate.Date)
                return Result<EmployeeResponse>.Failure("Дата перевода не может быть раньше даты приёма на работу.");

            var openAssignment = await _db.EmployeeDepartmentAssignments
                .Where(a => a.EmployeeId == employeeId && a.ValidTo == null)
                .FirstOrDefaultAsync(ct);

            if (openAssignment is not null)
            {
                if (openAssignment.DepartmentId == request.NewDepartmentId)
                    return Result<EmployeeResponse>.Conflict("Сотрудник уже числится в этом подразделении.");

                if (effectiveDate < openAssignment.ValidFrom)
                    return Result<EmployeeResponse>.Failure("Дата перевода не может быть раньше даты предыдущего назначения.");

                openAssignment.ValidTo = effectiveDate;
            }

            _db.EmployeeDepartmentAssignments.Add(new EmployeeDepartmentAssignment
            {
                EmployeeId = employeeId,
                DepartmentId = request.NewDepartmentId,
                ValidFrom = effectiveDate,
                ValidTo = null
            });

            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Сотрудник {Id} переведён в подразделение {DepartmentId} с {Date}",
                employeeId, request.NewDepartmentId, effectiveDate);

            return await GetByIdAsync(employeeId, ct);
        }

        public async Task<Result<EmployeeResponse>> TransferPositionAsync(
            int employeeId, TransferPositionRequest request, CancellationToken ct = default)
        {
            var employee = await _db.Employees.FindAsync(new object?[] { employeeId }, ct);
            if (employee is null)
                return Result<EmployeeResponse>.NotFound($"Сотрудник с id={employeeId} не найден.");

            if (!employee.IsActive)
                return Result<EmployeeResponse>.Conflict("Нельзя перевести уволенного сотрудника.");

            var newPosition = await _db.Positions.FindAsync(new object?[] { request.NewPositionId }, ct);
            if (newPosition is null)
                return Result<EmployeeResponse>.NotFound($"Должность с id={request.NewPositionId} не найдена.");

            var effectiveDate = request.EffectiveDate.Date;
            if (effectiveDate < employee.HireDate.Date)
                return Result<EmployeeResponse>.Failure("Дата перевода не может быть раньше даты приёма на работу.");

            var openAssignment = await _db.EmployeePositionAssignments
                .Where(a => a.EmployeeId == employeeId && a.ValidTo == null)
                .FirstOrDefaultAsync(ct);

            if (openAssignment is not null)
            {
                if (openAssignment.PositionId == request.NewPositionId)
                    return Result<EmployeeResponse>.Conflict("Сотрудник уже занимает эту должность.");

                if (effectiveDate < openAssignment.ValidFrom)
                    return Result<EmployeeResponse>.Failure("Дата перевода не может быть раньше даты предыдущего назначения.");

                openAssignment.ValidTo = effectiveDate;
            }

            _db.EmployeePositionAssignments.Add(new EmployeePositionAssignment
            {
                EmployeeId = employeeId,
                PositionId = request.NewPositionId,
                ValidFrom = effectiveDate,
                ValidTo = null
            });

            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Сотрудник {Id} переведён на должность {PositionId} с {Date}",
                employeeId, request.NewPositionId, effectiveDate);

            return await GetByIdAsync(employeeId, ct);
        }

        public async Task<Result<EmployeeResponse>> GetByIdAsync(int employeeId, CancellationToken ct = default)
        {
            var employee = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == employeeId, ct);
            if (employee is null)
                return Result<EmployeeResponse>.NotFound($"Сотрудник с id={employeeId} не найден.");

            var response = await BuildResponseAsync(employee, ct);
            return Result<EmployeeResponse>.Success(response);
        }

        public async Task<Result<List<EmployeeResponse>>> GetAllAsync(bool onlyActive, CancellationToken ct = default)
        {
            var query = _db.Employees.AsNoTracking().AsQueryable();
            if (onlyActive) query = query.Where(e => e.DismissalDate == null);

            var employees = await query.OrderBy(e => e.Surname).ThenBy(e => e.Name).ToListAsync(ct);

            var result = new List<EmployeeResponse>(employees.Count);
            foreach (var employee in employees)
            {
                result.Add(await BuildResponseAsync(employee, ct));
            }

            return Result<List<EmployeeResponse>>.Success(result);
        }

        private async Task<EmployeeResponse> BuildResponseAsync(Employee employee, CancellationToken ct)
        {
            var currentDept = await _db.EmployeeDepartmentAssignments.AsNoTracking()
                .Where(a => a.EmployeeId == employee.Id && a.ValidTo == null)
                .Include(a => a.Department)
                .FirstOrDefaultAsync(ct);

            var currentPos = await _db.EmployeePositionAssignments.AsNoTracking()
                .Where(a => a.EmployeeId == employee.Id && a.ValidTo == null)
                .Include(a => a.Position)
                .FirstOrDefaultAsync(ct);

            return new EmployeeResponse
            {
                Id = employee.Id,
                LastName = employee.Surname,
                FirstName = employee.Name,
                MiddleName = employee.Patronim,
                HireDate = employee.HireDate,
                DismissalDate = employee.DismissalDate,
                IsActive = employee.IsActive,
                CurrentDepartmentId = currentDept?.DepartmentId,
                CurrentDepartmentName = currentDept?.Department.Name,
                CurrentPositionId = currentPos?.PositionId,
                CurrentPositionTitle = currentPos?.Position.Title
            };
        }
    }
}
