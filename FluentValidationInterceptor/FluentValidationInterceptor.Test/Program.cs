using System.Reflection;
using FluentValidation;
using FluentValidationInterceptor;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddRequestValidation();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseRouting();
app.UseRequestValidation();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
