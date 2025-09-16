# ExcelFluently

A NuGet package for automating field validation in ASP.NET Core API or MVC applications using **FluentValidation**. This package simplifies the validation process by automatically intercepting requests and validating them according to rules defined in FluentValidation validators. No manual validation code is required in each controller or action.

## Features

- **Automatic validation** of models and DTOs configured with **FluentValidation**
- Eliminates repetitive validation code in each endpoint method
- **Automatic error responses** in JSON format when validation fails
- Easy configuration with single `AddRequestValidation` and `UseRequestValidation` methods
- Compatible with **ASP.NET Core Web API** and **MVC**

## Installation

Install the package via NuGet Package Manager or CLI:

```bash
dotnet add package ExcelFluently
```

## The package will automatically install these required dependencies:

- FluentValidation
- FluentValidation.DependencyInjectionExtensions
- Microsoft.AspNetCore.Http
- Microsoft.AspNetCore.Mvc.Core

# Quick Start

### 1. Configure Services (Program.cs)

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Register RequestValidation (automatically finds all validators)
builder.Services.AddRequestValidation();

var app = builder.Build();

```

### 2. Configure Middleware (Program.cs)

```csharp

// Configure the HTTP request pipeline ! important
app.UseRouting();

// Use RequestValidation middleware
app.UseRequestValidation();

app.UseAuthorization();
app.MapControllers();
app.Run();

```

# Usage Example

### 1. Create a DTO

```csharp

public class CreateUserDto
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }
}
```

### 2. Create a Validator

This package extends **FluentValidation**. To learn how to create validators, check the official documentation:

- 📚 [FluentValidation Official Documentation](https://docs.fluentvalidation.net/en/latest/index.html)
- 🎯 [Getting Started Guide](https://docs.fluentvalidation.net/en/latest/start.html)
- 🛠️ [Custom Validators](https://docs.fluentvalidation.net/en/latest/custom-validators.html)


```csharp
public class CreateUserValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(50).WithMessage("Name cannot exceed 50 characters.");

        RuleFor(x => x.Age)
            .GreaterThan(0).WithMessage("Age must be greater than 0.")
            .InclusiveBetween(18, 99).WithMessage("Age must be between 18 and 99.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
    }
}
```

### 3. Create a Controller

```csharp

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    [HttpPost]
    public IActionResult CreateUser([FromBody] CreateUserDto userDto)
    {
        // No manual validation needed - automatically validated by middleware
        return Ok(new { Message = "User created successfully", User = userDto });
    }
}
```

## Automatic Validation Response

When validation fails, the middleware returns a structured JSON response:

```json
{
  "title": "Validation errors occurred",
  "status": 400,
  "detail": "Please correct the specified errors and try again.",
  "instance": "/api/users",
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "errors": {
    "Name": ["Name is required."],
    "Age": ["Age must be greater than 0.", "Age must be between 18 and 99."],
    "Email": ["A valid email address is required."]
  }
}
```

# Supported HTTP Methods

The middleware automatically validates these HTTP methods:

- POST
- PUT
- PATCH

# Benefits

- Automation: No manual ModelState checks in controllers
- Clean Code: Eliminates repetitive validation code
- Consistency: Standardized validation responses across all endpoints
- Scalability: Easily add new validations by creating new validators
- Multi-assembly Support: Automatically discovers validators across all - - - referenced projects

# Dependencies

- This package automatically includes:
- FluentValidation (≥11.9.0)
- FluentValidation.DependencyInjectionExtensions (≥11.9.0)
- Microsoft.AspNetCore.App (shared framework)
- Microsoft.Extensions.DependencyInjection (≥8.0.0)

# Troubleshooting

- Validators Not Found
- Ensure your validators are in assemblies that are loaded at runtime.
- The middleware automatically scans all available assemblies.

# Validation Not Triggering

- Ensure DTOs are decorated with [FromBody] attribute
- Check that HTTP method is POST, PUT, or PATCH
- Verify validators are properly registered

# License

MIT License - see LICENSE file for details.

# Support

- GitHub: [https://github.com/Ivanproyectos](https://github.com/ivperez/ExcelFluently)
- LinkedIn: [https://www.linkedin.com/in/ivan-perez-tintaya/](https://www.linkedin.com/in/ivanproyectos/)
