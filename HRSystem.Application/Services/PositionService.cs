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
    public class PositionService : IPositionService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<PositionService> _logger;

        public PositionService(AppDbContext db, ILogger<PositionService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Result<PositionResponse>> CreateAsync(CreatePositionRequest request, CancellationToken ct = default)
        {
            var exists = await _db.Positions.AnyAsync(p => p.Title == request.Title, ct);
            if (exists)
            {
                _logger.LogWarning("Попытка создать дублирующуюся должность '{Title}'", request.Title);
                return Result<PositionResponse>.Conflict($"Должность с названием '{request.Title}' уже существует.");
            }

            var position = new Position { Title = request.Title };
            _db.Positions.Add(position);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Создана должность {Id} '{Title}'", position.Id, position.Title);
            return Result<PositionResponse>.Success(ToResponse(position));
        }

        public async Task<Result<PositionResponse>> RenameAsync(int id, RenamePositionRequest request, CancellationToken ct = default)
        {
            var position = await _db.Positions.FindAsync(new object?[] { id }, ct);
            if (position is null)
                return Result<PositionResponse>.NotFound($"Должность с id={id} не найдена.");

            var duplicate = await _db.Positions.AnyAsync(p => p.Id != id && p.Title == request.Title, ct);
            if (duplicate)
                return Result<PositionResponse>.Conflict($"Должность с названием '{request.Title}' уже существует.");

            position.Title = request.Title;
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Должность {Id} переименована в '{Title}'", id, request.Title);
            return Result<PositionResponse>.Success(ToResponse(position));
        }

        public async Task<Result<PositionResponse>> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var position = await _db.Positions.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
            return position is null
                ? Result<PositionResponse>.NotFound($"Должность с id={id} не найдена.")
                : Result<PositionResponse>.Success(ToResponse(position));
        }

        public async Task<Result<List<PositionResponse>>> GetAllAsync(CancellationToken ct = default)
        {
            var positions = await _db.Positions.AsNoTracking()
                .OrderBy(p => p.Title)
                .ToListAsync(ct);

            return Result<List<PositionResponse>>.Success(positions.Select(ToResponse).ToList());
        }

        public async Task<Result> DeleteAsync(int id, CancellationToken ct = default)
        {
            var position = await _db.Positions.FindAsync(new object?[] { id }, ct);
            if (position is null)
                return Result.NotFound($"Должность с id={id} не найдена.");

            var inUse = await _db.EmployeePositionAssignments.AnyAsync(a => a.PositionId == id && a.ValidTo == null, ct);
            if (inUse)
                return Result.Conflict("Нельзя удалить должность: на ней числятся действующие сотрудники.");

            _db.Positions.Remove(position);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("Удалена должность {Id}", id);
            return Result.Success();
        }

        private static PositionResponse ToResponse(Position p) => new()
        {
            Id = p.Id,
            Title = p.Title
        };
    }
}
