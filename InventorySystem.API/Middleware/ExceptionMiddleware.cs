using System.Net;
using System.Text.Json;
using InventorySystem.Application.Exceptions;

namespace InventorySystem.API.Middleware;

public class ExceptionMiddleware
{
	private readonly RequestDelegate _next;

	public ExceptionMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (Exception ex)
		{
			await HandleExceptionAsync(context, ex);
		}
	}

	private static async Task HandleExceptionAsync(
		HttpContext context,
		Exception exception)
	{
		context.Response.ContentType = "application/json";

		var response = new
		{
			success = false,
			message = exception.Message
		};

		switch (exception)
		{
			case ValidationException:
				context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
				break;

			case NotFoundException:
				context.Response.StatusCode = (int)HttpStatusCode.NotFound;
				break;

			case BusinessException:
				context.Response.StatusCode = (int)HttpStatusCode.Conflict;
				break;

			default:
				context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

				response = new
				{
					success = false,
					message = "Error interno del servidor"
				};
				break;
		}

		var json = JsonSerializer.Serialize(response);

		await context.Response.WriteAsync(json);
	}
}