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
    public class DepartmentService : IDepartmentService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<DepartmentService> _logger;

        public DepartmentService(AppDbContext db, ILogger<DepartmentService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Result<DepartmentResponse>> CreateAsync(CreateDepartmentRequest request, CancellationToken ct = default)
        {
            if (request.ParentDepartmentId.HasValue)
            {
                var parent = await _db.Departments.FindAsync(new object?[] { request.ParentDepartmentId }, ct);
                if (parent is null)
                    return Result<DepartmentResponse>.NotFound(
                        $"Родительское подразделение с id={request.ParentDepartmentId} не найдено.");

                if (parent.IsDissolved)
                    return Result<DepartmentResponse>.Conflict(
                        $"Родительское подразделение с id={request.ParentDepartmentId} распущено.");
            }

            var effectiveDate = (request.EffectiveDate ?? DateTime.UtcNow).Date;

            var department = new Department { Name = request.Name };
            _db.Departments.Add(department);
            await _db.SaveChangesAsync(ct); 

            _db.DepartmentHierarchies.Add(new DepartmentHierarchy
            {
                DepartmentId = department.Id,
                ParentDepartmentId = request.ParentDepartmentId,
                ValidFrom = effectiveDate,
                ValidTo = null
            });
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Создано подразделение {Id} '{Name}' (родитель: {ParentId})",
                department.Id, department.Name, request.ParentDepartmentId);

            return Result<DepartmentResponse>.Success(new DepartmentResponse
            {
                Id = department.Id,
                Name = department.Name,
                ParentDepartmentId = request.ParentDepartmentId,
                IsDissolved = false,
                DissolvedDate = null
            });
        }

        public async Task<Result<DepartmentResponse>> RenameAsync(int id, RenameDepartmentRequest request, CancellationToken ct = default)
        {
            var department = await _db.Departments.FindAsync(new object?[] { id }, ct);
            if (department is null)
                return Result<DepartmentResponse>.NotFound($"Подразделение с id={id} не найдено.");

            if (department.IsDissolved)
                return Result<DepartmentResponse>.Conflict("Нельзя переименовать распущенное подразделение.");

            department.Name = request.Name;
            await _db.SaveChangesAsync(ct);

            var parentId = await GetCurrentParentIdAsync(id, DateTime.UtcNow.Date, ct);

            _logger.LogInformation("Подразделение {Id} переименовано в '{Name}'", id, request.Name);

            return Result<DepartmentResponse>.Success(new DepartmentResponse
            {
                Id = department.Id,
                Name = department.Name,
                ParentDepartmentId = parentId,
                IsDissolved = false,
                DissolvedDate = null
            });
        }

        public async Task<Result<DepartmentResponse>> MoveAsync(int id, MoveDepartmentRequest request, CancellationToken ct = default)
        {
            var department = await _db.Departments.FindAsync(new object?[] { id }, ct);
            if (department is null)
                return Result<DepartmentResponse>.NotFound($"Подразделение с id={id} не найдено.");

            if (department.IsDissolved)
                return Result<DepartmentResponse>.Conflict("Нельзя перенести распущенное подразделение.");

            if (request.NewParentDepartmentId == id)
                return Result<DepartmentResponse>.Failure("Подразделение не может быть родителем самому себе.");

            if (request.NewParentDepartmentId.HasValue)
            {
                var newParent = await _db.Departments.FindAsync(new object?[] { request.NewParentDepartmentId }, ct);
                if (newParent is null)
                    return Result<DepartmentResponse>.NotFound(
                        $"Родительское подразделение с id={request.NewParentDepartmentId} не найдено.");

                if (newParent.IsDissolved)
                    return Result<DepartmentResponse>.Conflict(
                        $"Родительское подразделение с id={request.NewParentDepartmentId} распущено.");

                // защита от циклов: новый родитель не должен быть потомком перемещаемого подразделения
                var isDescendant = await IsDescendantAsync(id, request.NewParentDepartmentId.Value, request.EffectiveDate, ct);
                if (isDescendant)
                    return Result<DepartmentResponse>.Failure(
                        "Нельзя перенести подразделение в одно из своих собственных подчинённых подразделений.");
            }

            var effectiveDate = request.EffectiveDate.Date;

            var currentRecord = await _db.DepartmentHierarchies
                .Where(h => h.DepartmentId == id && h.ValidTo == null)
                .OrderByDescending(h => h.ValidFrom)
                .FirstOrDefaultAsync(ct);

            if (currentRecord is not null)
            {
                if (currentRecord.ValidFrom == effectiveDate)
                {
                    currentRecord.ParentDepartmentId = request.NewParentDepartmentId;
                    await _db.SaveChangesAsync(ct);

                    _logger.LogInformation("Подразделение {Id} перенесено к родителю {ParentId} с {Date}",
                        id, request.NewParentDepartmentId, effectiveDate);

                    return Result<DepartmentResponse>.Success(new DepartmentResponse
                    {
                        Id = department.Id,
                        Name = department.Name,
                        ParentDepartmentId = request.NewParentDepartmentId,
                        IsDissolved = false,
                        DissolvedDate = null
                    });
                }

                if (effectiveDate < currentRecord.ValidFrom)
                    return Result<DepartmentResponse>.Failure(
                        "Дата перевода не может быть раньше даты последнего изменения структуры этого подразделения.");

                currentRecord.ValidTo = effectiveDate;
            }

            _db.DepartmentHierarchies.Add(new DepartmentHierarchy
            {
                DepartmentId = id,
                ParentDepartmentId = request.NewParentDepartmentId,
                ValidFrom = effectiveDate,
                ValidTo = null
            });

            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Подразделение {Id} перенесено к родителю {ParentId} с {Date}",
                id, request.NewParentDepartmentId, effectiveDate);

            return Result<DepartmentResponse>.Success(new DepartmentResponse
            {
                Id = department.Id,
                Name = department.Name,
                ParentDepartmentId = request.NewParentDepartmentId,
                IsDissolved = false,
                DissolvedDate = null
            });
        }

        public async Task<Result<List<DepartmentResponse>>> GetAllAsync(CancellationToken ct = default)
        {
            var today = DateTime.UtcNow.Date;

            var departments = await _db.Departments.AsNoTracking().ToListAsync(ct);
            var currentParents = await _db.DepartmentHierarchies.AsNoTracking()
                .Where(h => h.ValidFrom <= today && (h.ValidTo == null || h.ValidTo > today))
                .ToDictionaryAsync(h => h.DepartmentId, h => h.ParentDepartmentId, ct);

            var result = departments.Select(d => new DepartmentResponse
            {
                Id = d.Id,
                Name = d.Name,
                ParentDepartmentId = currentParents.TryGetValue(d.Id, out var parentId) ? parentId : null,
                IsDissolved = d.IsDissolved,
                DissolvedDate = d.DissolvedDate
            }).ToList();

            return Result<List<DepartmentResponse>>.Success(result);
        }

        public async Task<Result<DepartmentResponse>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var department = await _db.Departments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, ct);
            if (department is null)
                return Result<DepartmentResponse>.NotFound($"Подразделение с id={id} не найдено.");

            var parentId = await GetCurrentParentIdAsync(id, DateTime.UtcNow.Date, ct);

            return Result<DepartmentResponse>.Success(new DepartmentResponse
            {
                Id = department.Id,
                Name = department.Name,
                ParentDepartmentId = parentId,
                IsDissolved = department.IsDissolved,
                DissolvedDate = department.DissolvedDate
            });
        }

        public async Task<Result<List<DepartmentTreeNode>>> GetStructureAsync(
            int? departmentId, DateTime asOfDate, CancellationToken ct = default)
        {
            var date = asOfDate.Date;

            if (departmentId.HasValue)
            {
                var exists = await _db.Departments.AnyAsync(d => d.Id == departmentId, ct);
                if (!exists)
                    return Result<List<DepartmentTreeNode>>.NotFound($"Подразделение с id={departmentId} не найдено.");
            }
            var hierarchyAtDate = await _db.DepartmentHierarchies.AsNoTracking()
                .Where(h => h.ValidFrom <= date && (h.ValidTo == null || h.ValidTo > date))
                .ToListAsync(ct);

            var departmentIds = hierarchyAtDate.Select(h => h.DepartmentId).ToHashSet();
            var departments = await _db.Departments.AsNoTracking()
                .Where(d => departmentIds.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id, ct);

            var employeeAssignments = await _db.EmployeeDepartmentAssignments.AsNoTracking()
                .Where(a => a.ValidFrom <= date && (a.ValidTo == null || a.ValidTo > date) && departmentIds.Contains(a.DepartmentId))
                .Include(a => a.Employee)
                .ToListAsync(ct);

            var employeesByDepartment = employeeAssignments
                .GroupBy(a => a.DepartmentId)
                .ToDictionary(g => g.Key, g => g.Select(a => a.Employee).ToList());

            var positionAssignments = await _db.EmployeePositionAssignments.AsNoTracking()
                .Where(a => a.ValidFrom <= date && (a.ValidTo == null || a.ValidTo > date))
                .Include(a => a.Position)
                .ToListAsync(ct);
            var positionByEmployee = positionAssignments.ToDictionary(a => a.EmployeeId, a => a.Position);

            var childrenByParent = hierarchyAtDate
                .GroupBy(h => h.ParentDepartmentId)
                .ToDictionary(g => g.Key, g => g.Select(h => h.DepartmentId).ToList());

            DepartmentTreeNode BuildNode(int id)
            {
                var dept = departments[id];
                var node = new DepartmentTreeNode
                {
                    Id = dept.Id,
                    Name = dept.Name
                };

                if (employeesByDepartment.TryGetValue(id, out var employees))
                {
                    node.Employees = employees.Select(emp => new EmployeeResponse
                    {
                        Id = emp.Id,
                        LastName = emp.Surname,
                        FirstName = emp.Name,
                        MiddleName = emp.Patronim,
                        HireDate = emp.HireDate,
                        DismissalDate = emp.DismissalDate,
                        IsActive = emp.DismissalDate == null,
                        CurrentDepartmentId = dept.Id,
                        CurrentDepartmentName = dept.Name,
                        CurrentPositionId = positionByEmployee.TryGetValue(emp.Id, out var pos) ? pos.Id : null,
                        CurrentPositionTitle = positionByEmployee.TryGetValue(emp.Id, out var pos2) ? pos2.Title : null
                    }).ToList();
                }

                if (childrenByParent.TryGetValue(id, out var childIds))
                {
                    node.Children = childIds.Select(BuildNode).ToList();
                }

                return node;
            }

            List<DepartmentTreeNode> roots;
            if (departmentId.HasValue)
            {
                if (!departments.ContainsKey(departmentId.Value))
                {
                    return Result<List<DepartmentTreeNode>>.Success(new List<DepartmentTreeNode>());
                }

                roots = new List<DepartmentTreeNode> { BuildNode(departmentId.Value) };
            }
            else
            {
                var rootIds = childrenByParent.TryGetValue(null, out var ids) ? ids : new List<int>();
                roots = rootIds.Select(BuildNode).ToList();
            }

            return Result<List<DepartmentTreeNode>>.Success(roots);
        }

        public async Task<Result<DepartmentResponse>> DismissAsync(int id, DismissDepartmentRequest request, CancellationToken ct = default)
        {
            var department = await _db.Departments.FindAsync(new object?[] { id }, ct);
            if (department is null)
                return Result<DepartmentResponse>.NotFound($"Подразделение с id={id} не найдено.");

            if (department.IsDissolved)
                return Result<DepartmentResponse>.Conflict("Подразделение уже распущено.");

            var effectiveDate = request.EffectiveDate.Date;
            var ownRecord = await _db.DepartmentHierarchies
                .Where(h => h.DepartmentId == id && h.ValidTo == null)
                .OrderByDescending(h => h.ValidFrom)
                .FirstOrDefaultAsync(ct);

            var grandParentId = ownRecord?.ParentDepartmentId;

            var childRecords = await _db.DepartmentHierarchies
                .Where(h => h.ParentDepartmentId == id && h.ValidTo == null)
                .ToListAsync(ct);

            var employeeAssignments = await _db.EmployeeDepartmentAssignments
                .Where(a => a.DepartmentId == id && a.ValidTo == null)
                .ToListAsync(ct);

            if (grandParentId is null && (childRecords.Count > 0 || employeeAssignments.Count > 0))
                return Result<DepartmentResponse>.Conflict(
                    "Нельзя распустить корневое подразделение, у которого есть действующие дочерние " +
                    "подразделения или сотрудники: их некому наследовать. Сначала перенесите их вручную " +
                    "(MoveAsync / TransferDepartmentAsync).");
            foreach (var child in childRecords)
            {
                if (effectiveDate < child.ValidFrom)
                    return Result<DepartmentResponse>.Failure(
                        $"Дата роспуска не может быть раньше даты последнего перевода дочернего подразделения {child.DepartmentId}.");
            }

            foreach (var assignment in employeeAssignments)
            {
                if (effectiveDate < assignment.ValidFrom)
                    return Result<DepartmentResponse>.Failure(
                        $"Дата роспуска не может быть раньше даты последнего перевода сотрудника {assignment.EmployeeId} в это подразделение.");
            }
            if (ownRecord is not null)
                ownRecord.ValidTo = effectiveDate;
            foreach (var child in childRecords)
            {
                child.ValidTo = effectiveDate;
                _db.DepartmentHierarchies.Add(new DepartmentHierarchy
                {
                    DepartmentId = child.DepartmentId,
                    ParentDepartmentId = grandParentId,
                    ValidFrom = effectiveDate,
                    ValidTo = null
                });
            }
            foreach (var assignment in employeeAssignments)
            {
                assignment.ValidTo = effectiveDate;
                _db.EmployeeDepartmentAssignments.Add(new EmployeeDepartmentAssignment
                {
                    EmployeeId = assignment.EmployeeId,
                    DepartmentId = grandParentId!.Value,
                    ValidFrom = effectiveDate,
                    ValidTo = null
                });
            }
            department.DissolvedDate = effectiveDate;

            await _db.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Подразделение {Id} распущено с {Date}. Дочерних подразделений перенесено: {ChildCount}, " +
                "сотрудников перенесено: {EmployeeCount}. Новый родитель для них: {GrandParentId}",
                id, effectiveDate, childRecords.Count, employeeAssignments.Count, grandParentId);

            return Result<DepartmentResponse>.Success(new DepartmentResponse
            {
                Id = department.Id,
                Name = department.Name,
                ParentDepartmentId = grandParentId,
                IsDissolved = true,
                DissolvedDate = department.DissolvedDate
            });
        }

        private async Task<int?> GetCurrentParentIdAsync(int departmentId, DateTime date, CancellationToken ct)
        {
            var record = await _db.DepartmentHierarchies.AsNoTracking()
                .Where(h => h.DepartmentId == departmentId && h.ValidFrom <= date && (h.ValidTo == null || h.ValidTo > date))
                .OrderByDescending(h => h.ValidFrom)
                .FirstOrDefaultAsync(ct);

            return record?.ParentDepartmentId;
        }

        private async Task<bool> IsDescendantAsync(int departmentId, int candidateAncestorId, DateTime date, CancellationToken ct)
        {
            var hierarchyAtDate = await _db.DepartmentHierarchies.AsNoTracking()
                .Where(h => h.ValidFrom <= date.Date && (h.ValidTo == null || h.ValidTo > date.Date))
                .ToListAsync(ct);

            var current = candidateAncestorId;
            var visited = new HashSet<int>();

            while (true)
            {
                if (current == departmentId) return true;
                if (!visited.Add(current)) return false;

                var parentRecord = hierarchyAtDate.FirstOrDefault(h => h.DepartmentId == current);
                if (parentRecord?.ParentDepartmentId is null) return false;

                current = parentRecord.ParentDepartmentId.Value;
            }
        }
    }
}
