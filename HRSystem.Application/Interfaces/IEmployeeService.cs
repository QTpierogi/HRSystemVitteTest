using HRSystem.Application;
using HRSystem.Application.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRSystem.Application.Interfaces
{
    public interface IEmployeeService
    {
        Task<Result<EmployeeResponse>> HireAsync(HireEmployeeRequest request, CancellationToken ct = default);

        Task<Result<EmployeeResponse>> DismissAsync(int employeeId, DismissEmployeeRequest request, CancellationToken ct = default);

        Task<Result<EmployeeResponse>> TransferDepartmentAsync(int employeeId, TransferDepartmentRequest request, CancellationToken ct = default);

        Task<Result<EmployeeResponse>> TransferPositionAsync(int employeeId, TransferPositionRequest request, CancellationToken ct = default);

        Task<Result<EmployeeResponse>> GetByIdAsync(int employeeId, CancellationToken ct = default);

        Task<Result<List<EmployeeResponse>>> GetAllAsync(bool onlyActive, CancellationToken ct = default);

    }
}
