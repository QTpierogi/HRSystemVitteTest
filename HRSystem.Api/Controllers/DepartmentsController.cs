using HRSystem.Application;
using HRSystem.Application.DTO;
using HRSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Api.Controllers
{
    [Route("api/departments")]
    public class DepartmentsController : ApiControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<DepartmentResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(CancellationToken ct)
            => ToActionResult(await _departmentService.GetAllAsync(ct));

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(DepartmentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
            => ToActionResult(await _departmentService.GetByIdAsync(id, ct));

        [HttpGet("structure")]
        [ProducesResponseType(typeof(List<DepartmentTreeNode>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetStructure(
            [FromQuery] int? departmentId, [FromQuery] DateTime? date, CancellationToken ct)
            => ToActionResult(await _departmentService.GetStructureAsync(departmentId, date ?? DateTime.UtcNow, ct));

        [HttpPost]
        [ProducesResponseType(typeof(DepartmentResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] CreateDepartmentRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var result = await _departmentService.CreateAsync(request, ct);
            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value)
                : ToActionResult(result);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(DepartmentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Rename(int id, [FromBody] RenameDepartmentRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            return ToActionResult(await _departmentService.RenameAsync(id, request, ct));
        }

        [HttpPost("{id:int}/move")]
        [ProducesResponseType(typeof(DepartmentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Move(int id, [FromBody] MoveDepartmentRequest request, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            return ToActionResult(await _departmentService.MoveAsync(id, request, ct));
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var result = await _departmentService.DismissAsync(id, new DismissDepartmentRequest(), ct);
            return result.IsSuccess ? NoContent() : ToActionResult(result);
        }
    }
}
