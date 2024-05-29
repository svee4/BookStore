using BookStore;
using BookStore.Database;
using BookStore.Infra.Startup;
using Immediate.Handlers.Shared;
using Immediate.Validations.Shared;

[assembly: Behaviors(typeof(ValidationBehavior<,>))]

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSqlite<BookStoreDbContext>("Filename=books.db");

builder.Services.AddHandlers();
builder.Services.AddBehaviors();

builder.Services.AddSwagger();

builder.Services.AddProblemDetailsHandler();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
}

app.MapBookStoreEndpoints();

app.Run();

public partial class Program { } // make Program visible for tests
