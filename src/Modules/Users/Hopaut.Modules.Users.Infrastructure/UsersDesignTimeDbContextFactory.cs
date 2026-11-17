using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Users.Infrastructure;

public sealed class UsersDesignTimeDbContextFactory() : ModuleDesignTimeDbContextFactory<UsersModuleDbContext>(UsersModuleDbContext.SchemaName)
{
    protected override UsersModuleDbContext Create(DbContextOptions<UsersModuleDbContext> options) => new(options, DesignTimePublisher.Instance);
}
