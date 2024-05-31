using System.Diagnostics;
using System.Text.Json;
using Immediate.Validations.Shared;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Infra.Middleware;

public static class ProblemDetailsMiddleware
{
	
	public static void Handle(ProblemDetailsContext context)
	{
		ArgumentNullException.ThrowIfNull(context);

		if (context.Exception is null) return;

		switch (context.Exception)
		{
			case ValidationException ex:
			{
				// patch: convert an Immediate.Validations validation failure into a 400 bad request

				context.ProblemDetails = CreateValidationProblemDetails(
					context: context.HttpContext,
					status: 400,
					detail: ex.Message,
					errors: ex.Errors
						.GroupBy(x => x.PropertyName)
						.ToDictionary(x => x.Key, x => x.Select(y => y.ErrorMessage).ToArray()));

				context.HttpContext.Response.StatusCode = 400;
				break;
			}

			case BadHttpRequestException { InnerException: JsonException { } inner }:
			{
				// patch: convert a json request body parse error into a 400 bad request
				context.ProblemDetails = CreateValidationProblemDetails(context.HttpContext, 400, inner.Message);
				context.HttpContext.Response.StatusCode = 400;
				break;
			}

			case { } ex:
			{
				var isDevelopment = context.HttpContext.RequestServices.GetRequiredService<IHostEnvironment>().IsDevelopment();

				context.ProblemDetails = CreateProblemDetails(
					context: context.HttpContext,
					status: 500,
					detail: isDevelopment ? $"'{ex.GetType().Name}':\n--->{ex.Message}" : $"{ex.GetType().Name}");

				context.ProblemDetails.Title = "Unhandled server error";
				context.HttpContext.Response.StatusCode = 500;

				var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<IProblemDetailsService>>();
				logger.LogError("Unhandled server error for TraceId '{TraceId}':\n{Error}", context.HttpContext.TraceIdentifier, context.Exception);

				break;
			}

			default: throw new UnreachableException();
		}
	}

	private static ProblemDetails CreateProblemDetails(HttpContext context, int status, string? detail = null) =>
		new()
		{
			Status = status,
			Detail = detail, 
			Extensions = new Dictionary<string, object?>() { ["TraceId"] = context.TraceIdentifier }
		};

	private static ValidationProblemDetails CreateValidationProblemDetails(
		HttpContext context, 
		int status,
		string? detail = null,
		Dictionary<string, string[]>? errors = null)
	{
		var o = errors is not null ? new ValidationProblemDetails(errors) : new ValidationProblemDetails();
		o.Status = status;
		o.Detail = detail;
		o.Extensions = new Dictionary<string, object?>() { ["TraceId"] = context.TraceIdentifier };
		return o;
	}
}
