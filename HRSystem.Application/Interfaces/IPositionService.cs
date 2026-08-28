using HRSystem.Application;
using HRSystem.Application.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRSystem.Application.Interfaces
{
    public interface IPositionService
    {
        Task<Result<PositionResponse>> CreateAsync(CreatePositionRequest request, CancellationToken ct = default);
        Task<Result<PositionResponse>> RenameAsync(int id, RenamePositionRequest request, CancellationToken ct = default);
        Task<Result<PositionResponse>> GetByIdAsync(int id, CancellationToken ct = default);
        Task<Result<List<PositionResponse>>> GetAllAsync(CancellationToken ct = default);
        Task<Result> DeleteAsync(int id, CancellationToken ct = default);
    }
}
