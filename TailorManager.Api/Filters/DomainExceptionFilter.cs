using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TailorManager.Api;

// Turns the exceptions from Core into the status codes in api.md, so the controllers need no try/catch:
// KeyNotFoundException → 404, InvalidOperationException (like InvalidOrderStatusException) → 409, ArgumentException → 400.
// The body is the message, so the website can show it.
public class DomainExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        int? statusCode = context.Exception switch
        {
            KeyNotFoundException => StatusCodes.Status404NotFound,
            InvalidOperationException => StatusCodes.Status409Conflict,
            ArgumentException => StatusCodes.Status400BadRequest,
            _ => null
        };
        if (statusCode is null)
        {
            return;
        }

        context.Result = new ObjectResult(context.Exception.Message) { StatusCode = statusCode };
        context.ExceptionHandled = true;
    }
}
