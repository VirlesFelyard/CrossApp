using Api.Endpoints;
using Api.Errors;
using Api.Contracts;
using Api.Validation;
using FluentValidation;
using Core.Abstractions;
using Infrastructure;
using Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddScoped<IValidator<SaveBookRequest>, SaveBookRequestValidator>();
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = false);
var rawConnection = builder.Configuration.GetConnectionString("AppDb") ?? "Data Source=data/app.db";
var connectionBuilder = new SqliteConnectionStringBuilder(rawConnection);
var databasePath = Path.IsPathRooted(connectionBuilder.DataSource)
    ? connectionBuilder.DataSource
    : Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, connectionBuilder.DataSource));
Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);
connectionBuilder.DataSource = databasePath;
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionBuilder.ConnectionString));
builder.Services.AddScoped<IAsyncBookCatalogStore, EfBookCatalogStore>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapBooks();
app.Run();

public partial class Program { }
