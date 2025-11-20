using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Fora.Api.Controllers.Base;

[ApiController]
[Route("[controller]")]
public abstract class BaseController : ControllerBase
{
    private ILogger Logger =>
        HttpContext?.RequestServices.GetService(typeof(ILogger<BaseController>)) as ILogger<BaseController>;

    protected IActionResult HandleResult<T>(ErrorOr<T> result)
    {
        if (result.IsError)
        {
            List<string> errorsDescription =
                ["This is a Handled Error!", .. result.Errors.Select(msg => msg.Description).ToList()];

            string errorDescription = string.Join(" | ", errorsDescription);
            var error = result.FirstError;

            Logger?.LogWarning(
                "Handled API error. Type: {Type}, Code: {Code}, Description: {Description}",
                error.Type,
                error.Code,
                errorDescription);

            return error.Type switch
            {
                ErrorType.Unauthorized => Unauthorized(new { error.Code, errorDescription }),
                ErrorType.Validation => BadRequest(new { error.Code, errorDescription }),
                ErrorType.Conflict => Conflict(new { error.Code, errorDescription }),
                ErrorType.NotFound => NotFound(new { error.Code, errorDescription }),
                _ => StatusCode(StatusCodes.Status500InternalServerError, new { error.Code, errorDescription })
            };
        }

        Logger?.LogInformation("Request processed successfully. Returning result type: {Type}", typeof(T).Name);

        return Ok(result.Value);
    }
}
