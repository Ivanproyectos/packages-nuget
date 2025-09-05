using System.Reflection;
using FluentValidation;
using FluentValidationInterceptor;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddFluentValidationInterceptor();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseRouting();
app.UseFluentValidationInterceptor();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
