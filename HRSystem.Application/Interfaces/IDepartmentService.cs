using HRSystem.Application;
using HRSystem.Application.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRSystem.Application.Interfaces
{
    public interface IDepartmentService
    {
        Task<Result<DepartmentResponse>> CreateAsync(CreateDepartmentRequest request, CancellationToken ct = default);

        Task<Result<DepartmentResponse>> RenameAsync(int id, RenameDepartmentRequest request, CancellationToken ct = default);

        Task<Result<DepartmentResponse>> MoveAsync(int id, MoveDepartmentRequest request, CancellationToken ct = default);

        Task<Result<List<DepartmentResponse>>> GetAllAsync(CancellationToken ct = default);

        Task<Result<DepartmentResponse>> GetByIdAsync(int id, CancellationToken ct = default);

        Task<Result<List<DepartmentTreeNode>>> GetStructureAsync(int? departmentId, DateTime asOfDate, CancellationToken ct = default);

        Task<Result<DepartmentResponse>> DismissAsync(int id, DismissDepartmentRequest request, CancellationToken ct = default);
    }
}
