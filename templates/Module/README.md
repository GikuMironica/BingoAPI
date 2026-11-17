# Module Scaffolding Guide

To create a new module, replace `MODULENAME` with your module name (e.g., `Identity`, `Posts`, `Media`) in the template files below and copy them into the solution structure.

## Structure

```
src/Modules/{MODULENAME}/
├── Hopaut.Modules.{MODULENAME}.Domain/          # Entities, aggregates, value objects, domain events
├── Hopaut.Modules.{MODULENAME}.Application/      # Commands, queries, handlers, validators
├── Hopaut.Modules.{MODULENAME}.Infrastructure/   # EF DbContext, repositories, external integrations
└── Hopaut.Modules.{MODULENAME}.Api/              # Minimal API endpoints, module registration

tests/Modules/{MODULENAME}/
└── Hopaut.Modules.{MODULENAME}.Tests/            # Unit + integration tests
```

## Steps

1. Copy the `.csproj.template` files, rename to `.csproj`, replace `MODULENAME`.
2. Create a `{MODULENAME}Module.cs` in the Api project implementing `IModuleEndpoints`.
3. Create a `{MODULENAME}DbContext` in the Infrastructure project with a dedicated schema.
4. Register the module in `Hopaut.Api.Host/Program.cs` via `builder.Services.Add{MODULENAME}Module(...)`, and add its Domain, Application and Infrastructure assemblies plus its `{MODULENAME}UnitOfWorkBehavior<,>` to the literal lists of the Host's `AddMediator(...)` call.
5. Map endpoints via `app.Map{MODULENAME}Endpoints()`.
6. Add the projects to `Bingo.sln`.

## Conventions

- **Domain**: No dependencies on EF Core, ASP.NET, or Hangfire. Use `AggregateRoot<TId>` with private constructors and static factories.
- **Application**: Mediator (source-generated) commands and queries returning `Result`/`Result<T>`; every command implements the module marker `I{MODULENAME}Command`, which selects `{MODULENAME}UnitOfWorkBehavior<,>`. Handlers and messages are `public`. No ASP.NET references, no `SaveChangesAsync`. Use `ICachedQuery` for cacheable queries.
- **Infrastructure**: EF Core persistence. Each module owns its own schema; its DbContext derives from `ModuleDbContext` and implements `I{MODULENAME}UnitOfWork`. Domain-event handlers are `public`.
- **Api**: Minimal API endpoints only. Use `IModuleEndpoints.MapEndpoints(...)`.

## CPM

All package versions are managed centrally via `Directory.Packages.props`. Do not specify versions in `.csproj` files.
