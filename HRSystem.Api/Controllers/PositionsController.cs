using HRSystem.Application;
using HRSystem.Application.DTO;
using HRSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Api.Controllers
{
    [Route("api/positions")]
    public class PositionsController : ApiControllerBase
    {
        private readonly IPositionService _positionService;

        public PositionsController(IPositionService positionService)
        {
            _positionService = positionService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<PositionResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
            => ToActionResult(await _positionService.GetAllAsync(ct));

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PositionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
            => ToActionResult(await _positionService.GetByIdAsync(id, ct));

        [HttpPost]
        [ProducesResponseType(typeof(PositionResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromBody] CreatePositionRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var result = await _positionService.CreateAsync(request, ct);
            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
                : ToActionResult(result);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(PositionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Rename(int id, [FromBody] RenamePositionRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            return ToActionResult(await _positionService.RenameAsync(id, request, ct));
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var result = await _positionService.DeleteAsync(id, ct);
            return result.IsSuccess ? NoContent() : ToActionResult(result);
        }
    }
}
