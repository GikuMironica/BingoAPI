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
4. Register the module in `Hopaut.Api.Host/Program.cs` via `builder.Services.Add{MODULENAME}Module(...)`.
5. Map endpoints via `app.Map{MODULENAME}Endpoints()`.
6. Add the projects to `Bingo.sln`.

## Conventions

- **Domain**: No dependencies on EF Core, ASP.NET, or Hangfire. Use `AggregateRoot<TId>` with private constructors and static factories.
- **Application**: MediatR handlers only. No ASP.NET references. Use `ICachedQuery` for cacheable queries.
- **Infrastructure**: EF Core persistence. Each module owns its own schema. Implement `IUnitOfWork`.
- **Api**: Minimal API endpoints only. Use `IModuleEndpoints.MapEndpoints(...)`.

## CPM

All package versions are managed centrally via `Directory.Packages.props`. Do not specify versions in `.csproj` files.
