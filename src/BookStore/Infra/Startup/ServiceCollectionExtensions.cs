using System.Text.Json;
using Immediate.Validations.Shared;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Infra.Startup;

public static class ServiceCollectionExtensions
{
	public static void AddProblemDetailsHandler(this IServiceCollection services)
	{
		services.AddProblemDetails(options =>
			options.CustomizeProblemDetails = context =>
			{
				if (context.Exception is null) return;
				
				switch (context.Exception)
				{
					case ValidationException ex:
						context.ProblemDetails = CreateValidationProblemDetails(
							context: context.HttpContext, 
							status: 400, 
							detail: ex.Message, 
							errors: ex.Errors
								.GroupBy(x => x.PropertyName)
								.ToDictionary(x => x.Key, x => x.Select(y => y.ErrorMessage).ToArray()));
						
						context.HttpContext.Response.StatusCode = 400;
						break;

					case BadHttpRequestException { InnerException: JsonException { } inner }:
						// short-term hack to format a bad request body error nicely - this isn't handled by Immediate.Handlers yet
						context.ProblemDetails = CreateValidationProblemDetails(context.HttpContext, 400, inner.Message);
						context.HttpContext.Response.StatusCode = 400;
						break;
					
					case { } ex:
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
				
			});

		static ProblemDetails CreateProblemDetails(HttpContext context, int status, string? detail = null) =>
			new() { 
				Status = status, 
				Detail = detail, 
				Extensions = new Dictionary<string, object?>()
			{
				["TraceId"] = context.TraceIdentifier
			}};
		
		static ValidationProblemDetails CreateValidationProblemDetails(HttpContext context, int status, string? detail = null, Dictionary<string, string[]>? errors = null)
		{
			var o = errors is not null ? new ValidationProblemDetails(errors) : new ValidationProblemDetails();
			o.Status = status;
			o.Detail = detail;
			o.Extensions = new Dictionary<string, object?>()
			{
				["TraceId"] = context.TraceIdentifier	
			};
			return o;
		}
	}

	public static void AddSwagger(this IServiceCollection services)
	{
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen(options => options.CustomSchemaIds(x => x.FullName?.Replace("+", ".", StringComparison.Ordinal)));
	}

	public static void UseSwagger(this IApplicationBuilder app)
	{
		SwaggerBuilderExtensions.UseSwagger(app);
		app.UseSwaggerUI(options =>
		{
			// serve swagger at root
			options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
			options.RoutePrefix = "";
		});
	}
}
