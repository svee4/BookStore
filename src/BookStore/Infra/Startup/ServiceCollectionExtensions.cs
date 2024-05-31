using BookStore.Infra.Middleware;

namespace BookStore.Infra.Startup;

public static class ServiceCollectionExtensions
{
	
	public static void AddProblemDetailsHandler(this IServiceCollection services) => 
		services.AddProblemDetails(options => options.CustomizeProblemDetails = ProblemDetailsMiddleware.Handle);

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
