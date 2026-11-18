using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace Hopaut.BuildingBlocks.Infrastructure.Persistence;

public static class ModuleDbContextOptionsExtensions
{
    /// <summary>
    /// The one Npgsql configuration shared by the runtime registration and the design-time factory of a module DbContext:
    /// the migrations history table lives in the module's own schema.
    /// </summary>
    public static DbContextOptionsBuilder UseModuleNpgsql(
        this DbContextOptionsBuilder options,
        string? connectionString,
        string schema,
        Action<NpgsqlDbContextOptionsBuilder>? configureNpgsql = null) =>
        options.UseNpgsql(connectionString, npgsql =>
        {
            npgsql.MigrationsHistoryTable(HistoryRepository.DefaultTableName, schema);
            configureNpgsql?.Invoke(npgsql);
        });
}
