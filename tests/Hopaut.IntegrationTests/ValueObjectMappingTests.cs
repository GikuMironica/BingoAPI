using AwesomeAssertions.Execution;
using Hopaut.BuildingBlocks.Infrastructure;
using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Hopaut.IntegrationTests.Infrastructure;
using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Hopaut.IntegrationTests;

/// <summary>Plan Step 8.1: the shared value-object mappings round-trip through PostgreSQL in the columns they promise.</summary>
[Collection(HostCollection.Name)]
public sealed class ValueObjectMappingTests(HopautApiFactory factory)
{
    private static readonly DateTimeOffset Doors = new(2024, 1, 9, 18, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GivenValueObjects_WhenSavedAndReadBack_ThenTheyRoundTripThroughTheirColumns()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var database = new NpgsqlConnectionStringBuilder(factory.ConnectionString) { Database = "value_objects_" + Guid.NewGuid().ToString("N") };
        var options = new DbContextOptionsBuilder<MappingContext>().UseNpgsql(database.ConnectionString).Options;
        var fee = Money.Create(12.5m, Currency.Euro).Value;
        var evening = TimeRange.Create(Doors, Doors.AddHours(5)).Value;
        var openEnded = TimeRange.Create(Doors, null).Value;
        await using (var setup = new MappingContext(options))
        {
            await setup.Database.EnsureCreatedAsync(ct);
            setup.Tickets.AddRange(
                new Ticket(1, fee, deposit: null, evening, RatingValue.Create(4).Value, EmailAddress.Create("alice@hopaut.app").Value),
                new Ticket(2, Money.Zero(Currency.Create("chf").Value), deposit: fee, openEnded, RatingValue.Create(1).Value, EmailAddress.Create("bob@hopaut.app").Value));
            await setup.SaveChangesAsync(ct);
        }

        try
        {
            // Act
            await using var read = new MappingContext(options);
            var tickets = await read.Tickets.AsNoTracking().OrderBy(t => t.Id).ToListAsync(ct);
            var columns = await read.Database
                .SqlQueryRaw<string>("SELECT column_name || ':' || data_type AS \"Value\" FROM information_schema.columns WHERE table_name = 'tickets'")
                .ToListAsync(ct);

            // Assert
            using (new AssertionScope())
            {
                tickets[0].Price.Should().Be(fee);
                tickets[0].Deposit.Should().BeNull();
                tickets[0].Validity.Should().Be(evening);
                tickets[0].Rating.Value.Should().Be(4);
                tickets[0].Contact.Value.Should().Be("alice@hopaut.app");
                tickets[1].Price.Should().Be(Money.Zero(Currency.Create("CHF").Value));
                tickets[1].Deposit.Should().Be(fee);
                tickets[1].Validity.End.Should().BeNull();
                columns.Should().Contain(
                [
                    "price_amount:numeric", "price_currency:character", "deposit_amount:numeric", "deposit_currency:character",
                    "validity_start:timestamp with time zone", "validity_end:timestamp with time zone", "Rating:integer", "Contact:text",
                ]);
            }
        }
        finally
        {
            await using var cleanup = new MappingContext(options);
            await cleanup.Database.EnsureDeletedAsync(ct);
        }
    }

    private sealed class Ticket
    {
        private Ticket() { } // EF Core

        public Ticket(int id, Money price, Money? deposit, TimeRange validity, RatingValue rating, EmailAddress contact) =>
            (Id, Price, Deposit, Validity, Rating, Contact) = (id, price, deposit, validity, rating, contact);

        public int Id { get; private set; }

        public Money Price { get; private set; }

        public Money? Deposit { get; private set; }

        public TimeRange Validity { get; private set; }

        public RatingValue Rating { get; private set; }

        public EmailAddress Contact { get; private set; }
    }

    private sealed class MappingContext(DbContextOptions<MappingContext> options) : DbContext(options)
    {
        public DbSet<Ticket> Tickets => Set<Ticket>();

        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            modelBuilder.Entity<Ticket>(ticket =>
            {
                ticket.ToTable("tickets");
                ticket.Property(t => t.Id).ValueGeneratedNever();
                ticket.ComplexProperty(t => t.Price, price => price.MapMoney("price"));
                ticket.ComplexProperty(t => t.Deposit, deposit => deposit.MapMoney("deposit"));
                ticket.ComplexProperty(t => t.Validity, validity => validity.MapTimeRange("validity"));
                ticket.Property(t => t.Rating).HasConversion<ValueObjectConverter<RatingValue, int>>();
                ticket.Property(t => t.Contact).HasConversion<ValueObjectConverter<EmailAddress, string>>();
            });
    }
}
