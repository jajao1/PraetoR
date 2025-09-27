# PraetoR.FluentValidation 🛡️✨

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

The official extension to integrate [FluentValidation](https://docs.fluentvalidation.net/en/latest/) into your [PraetoR](https://github.com/YOUR-GITHUB-USERNAME/YOUR-REPOSITORY) command and event pipeline. Ensure your commands and queries are always validated before being processed by their respective handlers.

## Overview

`PraetoR.FluentValidation` provides `IPipelineBehavior` implementations that intercept `ICommand` and `ICommand<TResponse>` dispatched through `IPraetoR`. It automatically locates and executes all registered FluentValidation validators (`AbstractValidator<TCommand>`) for the given command. If any validation rule fails, a `FluentValidation.ValidationException` is thrown, preventing the command from reaching its handler.

This library is essential for implementing robust and decoupled validation within your PraetoR-based architecture.

## Installation

The easiest way to add `PraetoR.FluentValidation` to your project is via the NuGet Package Manager.

```bash
dotnet add package PraetoR.FluentValidation
```

You will also need to have `PraetoR` and `FluentValidation.DependencyInjectionExtensions` installed in your application project:

```bash
dotnet add package PraetoR
dotnet add package FluentValidation.DependencyInjectionExtensions
```

## Configuration

Configure `PraetoR.FluentValidation` in your `Program.cs` (or `Startup.cs` for .NET Framework) alongside the registration of PraetoR and your validators.

```csharp
using System.Reflection;
using FluentValidation;
using PraetoR.Extensions; // For AddPraetoR
using PraetoR.FluentValidation; // For AddPraetoRFluentValidation

var builder = WebApplication.CreateBuilder(args);

// ... other services

// 1. Register PraetoR and all its handlers within the current assembly
builder.Services.AddPraetoR(Assembly.GetExecutingAssembly());

// 2. Add the Validation Pipeline Behavior to PraetoR
//    This ensures all commands will go through validation
builder.Services.AddPraetoRFluentValidation(Assembly.GetExecutingAssembly());

// ... rest of your application setup and build
```

## Example Usage

### 1. Define Your Commands and Validators

Create your commands (with or without a return value) and their respective validators inheriting from `AbstractValidator<TCommand>`.

```csharp
// Command with a return value
public class CreateUserCommand : ICommand<Guid>
{
    public string UserName { get; }
    public string Email { get; }
    public CreateUserCommand(string userName, string email) { UserName = userName; Email = email; }
}

// Validator for CreateUserCommand
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("User name is required.")
            .MinimumLength(3).WithMessage("User name must be at least 3 characters long.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}

// Command without a return value
public class DeleteUserCommand : ICommand
{
    public Guid UserId { get; }
    public DeleteUserCommand(Guid userId) => UserId = userId;
}

// Validator for DeleteUserCommand
public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID cannot be empty.");
    }
}
```

### 2. Dispatch Your Commands via IPraetoR

In your application logic (e.g., in an API Controller), inject `IPraetoR` and dispatch your commands. Validation will be executed automatically.

```csharp
using Microsoft.AspNetCore.Mvc;
using PraetoR.Abstractions;
using FluentValidation; // To catch ValidationException
using System.Linq; // For Select extension method

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IPraetoR _PraetoR;

    public UsersController(IPraetoR PraetoR)
    {
        _PraetoR = PraetoR;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var command = new CreateUserCommand(request.UserName, request.Email);
        try
        {
            var newUserId = await _PraetoR.Send(command); // Validation occurs here!
            return Ok(new { Id = newUserId });
        }
        catch (ValidationException ex)
        {
            // Catch validation failures and return a Bad Request
            return BadRequest(ex.Errors.Select(e => new { Field = e.PropertyName, Message = e.ErrorMessage }));
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var command = new DeleteUserCommand(id);
        try
        {
            await _PraetoR.Send(command); // Validation occurs here!
            return NoContent();
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Errors.Select(e => new { Field = e.PropertyName, Message = e.ErrorMessage }));
        }
    }
}
```

## How It Works (Internally)

`PraetoR.FluentValidation` registers two distinct `IPipelineBehavior` implementations:

* `ValidationBehavior<TRequest, TResponse>`: Used for commands that implement `ICommand<TResponse>`.
* `ValidationBehaviorNoResult<TRequest>`: Used for commands that implement only `ICommand`.

Both behaviors intercept the command, resolve all `IValidator<TRequest>` for that specific command from the DI container, and execute them. If any failures are found, a `ValidationException` is thrown, halting the pipeline's execution.

## Contributing

Contributions are welcome! If you find a bug or have a suggestion for an improvement, feel free to open an issue or submit a Pull Request.

## License

This project is licensed under the **MIT License**. See the `LICENSE` file for more details.