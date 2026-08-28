using HRSystem.Application;
using Microsoft.AspNetCore.Mvc;

namespace HRSystem.Api.Controllers
{

    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected IActionResult ToActionResult(Result result)
        {
            if (result.IsSuccess) return Ok();

            return result.ErrorType switch
            {
                ResultErrorType.NotFound => NotFound(new { error = result.Error }),
                ResultErrorType.Conflict => Conflict(new { error = result.Error }),
                ResultErrorType.Validation => BadRequest(new { error = result.Error }),
                _ => Problem(detail: result.Error, statusCode: StatusCodes.Status500InternalServerError)
            };
        }

        protected IActionResult ToActionResult<T>(Result<T> result)
        {
            if (result.IsSuccess) return Ok(result.Value);

            return result.ErrorType switch
            {
                ResultErrorType.NotFound => NotFound(new { error = result.Error }),
                ResultErrorType.Conflict => Conflict(new { error = result.Error }),
                ResultErrorType.Validation => BadRequest(new { error = result.Error }),
                _ => Problem(detail: result.Error, statusCode: StatusCodes.Status500InternalServerError)
            };
        }
    }
}
