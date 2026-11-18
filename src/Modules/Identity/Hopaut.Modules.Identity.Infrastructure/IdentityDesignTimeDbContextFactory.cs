using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Identity.Infrastructure;

public sealed class IdentityDesignTimeDbContextFactory()
    : ModuleDesignTimeDbContextFactory<IdentityModuleDbContext>(IdentityModuleDbContext.SchemaName)
{
    protected override IdentityModuleDbContext Create(DbContextOptions<IdentityModuleDbContext> options) => new(options);
}
