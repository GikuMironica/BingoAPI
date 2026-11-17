using Hopaut.SharedKernel;

// Deliberately broken types used only by RuleSelfTests to prove that each rule reports what it claims to detect.
// They live in the test assembly, which the production rules never scan.
#pragma warning disable CA1822 // fixture members are intentionally instance members

namespace Hopaut.ArchitectureTests.Fixtures.BadDomain
{
    public sealed class UsesEfCore
    {
        public Microsoft.EntityFrameworkCore.DbContext? Context { get; init; }
    }

    public sealed class UsesAspNetCoreIdentity : Microsoft.AspNetCore.Identity.IdentityUser;

    public sealed class UsesAnotherModule
    {
        public Hopaut.Modules.Ratings.Domain.Rating? Rating { get; init; }
    }

    public sealed class UsesDataAnnotations
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string Name { get; init; } = string.Empty;
    }
}

namespace Hopaut.ArchitectureTests.Fixtures.GoodDomain
{
    public sealed class CleanAggregate : AggregateRoot<int>
    {
        private readonly List<int> _items = [];

        private CleanAggregate()
        {
        }

        public string Name { get; private set; } = string.Empty;

        public IReadOnlyList<int> Items => _items;

        public static CleanAggregate Create(string name) => new() { Name = name };
    }
}

namespace Hopaut.ArchitectureTests.Fixtures.BadModel
{
    public sealed class LeakyAggregate : AggregateRoot<int>
    {
        public LeakyAggregate()
        {
        }

        public string Name { get; set; } = string.Empty;

        public List<int> Items { get; } = [];
    }

    public sealed class PublicEventRaiser : IHasDomainEvents
    {
        public IReadOnlyList<IDomainEvent> DomainEvents => [];

        public void ClearDomainEvents()
        {
        }

        public void Raise(IDomainEvent domainEvent)
        {
        }
    }
}

namespace Hopaut.ArchitectureTests.Fixtures.GoodMessaging
{
    public interface IFixturesCommand : Mediator.IBaseCommand;

    public sealed record GoodCommand : Mediator.ICommand<Result>, IFixturesCommand;

    public sealed class GoodCommandHandler : Mediator.ICommandHandler<GoodCommand, Result>
    {
        public ValueTask<Result> Handle(GoodCommand command, CancellationToken cancellationToken) => ValueTask.FromResult(Result.Success());
    }
}

namespace Hopaut.ArchitectureTests.Fixtures.BadMessaging
{
    public interface IOtherCommand : Mediator.IBaseCommand;

    public sealed record ReturnsBool : Mediator.ICommand<bool>, GoodMessaging.IFixturesCommand;

    public sealed record WithoutMarker : Mediator.ICommand<Result>;

    public sealed record WithTwoMarkers : Mediator.ICommand<Result>, GoodMessaging.IFixturesCommand, IOtherCommand;

    internal sealed class HiddenHandler : Mediator.ICommandHandler<WithoutMarker, Result>
    {
        public ValueTask<Result> Handle(WithoutMarker command, CancellationToken cancellationToken) => ValueTask.FromResult(Result.Success());
    }
}
