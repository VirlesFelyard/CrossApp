using Api.Endpoints;
using Api.Errors;
using Api.Contracts;
using Api.Validation;
using FluentValidation;
using Core.Abstractions;
using Core.Storage;
using Microsoft.AspNetCore.Routing;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddScoped<IValidator<SaveBookRequest>, SaveBookRequestValidator>();
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = false);
builder.Services.AddSingleton<IAsyncBookCatalogStore, InMemoryBookCatalogStore>();

var app = builder.Build();

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
