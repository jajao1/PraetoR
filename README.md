# PraetoR 🏛️

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

PraetoR is a lightweight, in-process command and event dispatcher for .NET. It helps you decouple your application's components by enforcing a clean separation between issuing commands and handling business logic.

The name comes from the PraetoRs of ancient Rome, magistrates given the authority to lead armies and issue commands. This library acts as the central authority in your application, receiving commands and ensuring they are executed by the correct handler.

## Core Concepts

- **`IPraetoR`**: The central service you inject into your classes to send commands and publish dicta (events).
- **Command**: A message that is sent to a **single handler** and represents an intention to change the system's state or retrieve data.
  - `IOperation<TResult>`: For commands that return a value (Queries or Commands).
  - `IOperation`: For commands that do not return a value.
- **Dictum (Event)**: A formal pronouncement or decree. It is published to **multiple handlers** (zero or more) to notify other parts of the system that something has happened.
  - `IDictum`: The marker interface for all dicta.

## Installation

The easiest way to add PraetoR to your project is via the NuGet package manager.

```bash
dotnet add package PraetoR
```

## Getting Started

Let's walk through a complete flow: a command to create a user and a dictum to notify other parts of the system about this creation.

### 1. Define your Commands and Dicta

Create classes that represent the operations and events in your system.

**CreateUserCommand.cs** (A command that returns the ID of the new user)
```csharp
using PraetoR.Abstractions;

public class CreateUserCommand : IOperation<Guid>
{
    public string UserName { get; }
    public CreateUserCommand(string userName) => UserName = userName;
}
```

**UserCreatedDictum.cs** (A dictum that carries information about the created user)
```csharp
using PraetoR.Abstractions;

public class UserCreatedDictum : IDictum
{
    public Guid UserId { get; }
    public UserCreatedDictum(Guid userId) => UserId = userId;
}
```

### 2. Implement Handlers

Create classes that contain the logic to process your commands and dicta.

**CreateUserCommandHandler.cs**
```csharp
using PraetoR.Abstractions;

public class CreateUserCommandHandler : IOperation Handler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _userRepository;
    private readonly IPraetoR _PraetoR;

    public CreateUserCommandHandler(IUserRepository userRepository, IPraetoR PraetoR)
    {
        _userRepository = userRepository;
        _PraetoR = PraetoR;
    }

    public async Task<Guid> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        // Business logic to create the user...
        var newUser = new User { Name = command.UserName };
        var newUserId = await _userRepository.Add(newUser);

        // Publishes a dictum to notify other parts of the system
        await _PraetoR.Publish(new UserCreatedDictum(newUserId), cancellationToken);

        return newUserId;
    }
}
```

**SendWelcomeEmailHandler.cs** (A handler that reacts to the dictum)
```csharp
using PraetoR.Abstractions;

public class SendWelcomeEmailHandler : IDictumHandler<UserCreatedDictum>
{
    public Task Handle(UserCreatedDictum dictum, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Sending welcome email to user {dictum.UserId}...");
        // Email sending logic goes here...
        return Task.CompletedTask;
    }
}
```

### 3. Register PraetoR with Dependency Injection

In your `Program.cs` (or `Startup.cs`), call the `AddPraetoR` extension method, specifying which assembly it should scan to find your handlers.

**Program.cs**
```csharp
using System.Reflection;
using PraetoR.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Registers PraetoR and all handlers in the current assembly
builder.Services.AddPraetoR(Assembly.GetExecutingAssembly());

// ... register your other dependencies, like IUserRepository
// builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();
// ...
```

### 4. Use IPraetoR to Dispatch

Now, in your classes (like API controllers), you can inject `IPraetoR` and use it to send commands, keeping your code clean and decoupled from the business logic.

**UsersController.cs**
```csharp
using Microsoft.AspNetCore.Mvc;
using PraetoR.Abstractions;

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
        var command = new CreateUserCommand(request.UserName);
        var newUserId = await _PraetoR.Send(command); // Send the command
        
        return Ok(new { Id = newUserId });
    }
}
```

## API Overview

| Interface | Description |
| :--- | :--- |
| `IPraetoR` | The central dispatcher for sending commands and publishing dicta. |
| `IOperation<TResult>` | Represents a command that expects a response of type `TResult`. |
| `IOperation` | Represents a command that executes an action but returns no value. |
| `IOperationHandler<...>` | Defines the class that handles an `IOperation`. |
| `IDictum` | A marker interface for events (dicta) that can be published. |
| `IDictumHandler<TDictum>`| Defines the class that handles (listens for) an `IDictum`. |

## Contributing

Contributions are welcome! If you find a bug or have a suggestion for an improvement, feel free to open an issue or submit a Pull Request.

## License

This project is licensed under the **MIT License**. See the `LICENSE` file for more details.