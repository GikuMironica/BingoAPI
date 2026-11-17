# Module Scaffolding Guide

To create a new module, replace `MODULENAME` with your module name (e.g., `Identity`, `Posts`, `Media`) in the template files below and copy them into the solution structure.

## Structure

```
src/Modules/{MODULENAME}/
├── Hopaut.Modules.{MODULENAME}.Domain/          # Entities, aggregates, value objects, domain events
├── Hopaut.Modules.{MODULENAME}.Application/      # Commands, queries, handlers, validators, ports
├── Hopaut.Modules.{MODULENAME}.Contracts/        # Optional: public surface for other modules (integration events, read-only query interfaces + DTOs)
├── Hopaut.Modules.{MODULENAME}.Infrastructure/   # EF DbContext, repositories, external integrations, module registration (Add{MODULENAME}Module)
└── Hopaut.Modules.{MODULENAME}.Api/              # Minimal API endpoints (Map{MODULENAME}Endpoints)

tests/Modules/{MODULENAME}/
└── Hopaut.Modules.{MODULENAME}.UnitTests/        # Domain + Application unit tests (xUnit v3, AwesomeAssertions, NSubstitute, FakeTimeProvider)
```

## Steps

1. Copy the `.csproj.template` files, rename to `.csproj`, replace `MODULENAME`. Create the Contracts project only when the module first exposes a type to another module (decision D-023 in `TODO.md`) and reference it from the module's Infrastructure.
2. Create a `{MODULENAME}Module.cs` in the Api project implementing `IModuleEndpoints`, and a `{MODULENAME}Endpoints` class with the `Map{MODULENAME}Endpoints(this IEndpointRouteBuilder)` extension.
3. Create a `{MODULENAME}ModuleDbContext` in the Infrastructure project with a dedicated schema, and a `{MODULENAME}ModuleInstaller` with the `Add{MODULENAME}Module(this IServiceCollection, IConfiguration)` extension that registers the module's services.
4. Reference the module's Api and Infrastructure projects from `Hopaut.Api.Host`, register the module in `Program.cs` via `builder.Services.Add{MODULENAME}Module(...)`, and add its Domain, Application and Infrastructure assemblies plus its `{MODULENAME}UnitOfWorkBehavior<,>` to the literal lists of the Host's `AddMediator(...)` call.
5. Map endpoints via `app.Map{MODULENAME}Endpoints()`.
6. Add the projects to `Bingo.sln`.

## Conventions

- **Domain**: No dependencies on EF Core, ASP.NET, or Hangfire. Use `AggregateRoot<TId>` with private constructors and static factories.
- **Application**: Mediator (source-generated) commands and queries returning `Result`/`Result<T>`; every command implements the module marker `I{MODULENAME}Command`, which selects `{MODULENAME}UnitOfWorkBehavior<,>`. Handlers and messages are `public`. No ASP.NET references, no `SaveChangesAsync`. Use `ICachedQuery` for cacheable queries.
- **Contracts**: Integration events and read-only query interfaces with their DTOs; references only `Hopaut.SharedKernel` and `Hopaut.IntegrationEvents`. Other modules may reference this project and nothing else of the module; a cross-module query interface is implemented in the owning module's Infrastructure.
- **Infrastructure**: EF Core persistence and the module's service registration. Each module owns its own schema; its DbContext derives from `ModuleDbContext` and implements `I{MODULENAME}UnitOfWork`. Domain-event handlers are `public`.
- **Api**: Minimal API endpoints only. Use `IModuleEndpoints.MapEndpoints(...)`. References its Application project and `Hopaut.BuildingBlocks.Api`, never its Domain or Infrastructure.

## CPM

All package versions are managed centrally via `Directory.Packages.props`. Do not specify versions in `.csproj` files.
