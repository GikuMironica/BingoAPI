using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hopaut.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Column layouts for the multi-value value objects mapped as EF Core complex types, e.g.
/// <c>entity.ComplexProperty(e => e.EntranceFee, fee => fee.MapMoney("entrance_fee"))</c>. Single-value value objects use
/// <see cref="ValueObjectConverter{TValueObject, TValue}"/>.
/// </summary>
public static class ValueObjectMappingExtensions
{
    /// <summary><c>{prefix}_amount</c> as <c>numeric(10,2)</c> and <c>{prefix}_currency</c> as a three-character code.</summary>
    public static ComplexPropertyBuilder<Money> MapMoney(this ComplexPropertyBuilder<Money> money, string columnPrefix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(columnPrefix);

        money.Property(m => m.Amount).HasColumnName($"{columnPrefix}_amount").HasPrecision(10, 2);
        money.Property(m => m.Currency)
            .HasConversion<ValueObjectConverter<Currency, string>>()
            .HasColumnName($"{columnPrefix}_currency")
            .HasMaxLength(3)
            .IsFixedLength();
        return money;
    }

    /// <summary><c>{prefix}_start</c> and <c>{prefix}_end</c> as <c>timestamp with time zone</c> (the end is optional).</summary>
    public static ComplexPropertyBuilder<TimeRange> MapTimeRange(this ComplexPropertyBuilder<TimeRange> range, string columnPrefix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(columnPrefix);

        range.Property(r => r.Start).HasColumnName($"{columnPrefix}_start");
        range.Property(r => r.End).HasColumnName($"{columnPrefix}_end");
        return range;
    }
}
