using BookStore.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSqlite<BookStoreDbContext>("Filename=books.db");

builder.Services.AddControllers();

builder.Services.AddSwaggerGen();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI(options =>
	{
		// serve swagger at root
		options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
		options.RoutePrefix = "";
	});
}

//app.UseHttpsRedirection();

app.MapControllers();

app.Run();
