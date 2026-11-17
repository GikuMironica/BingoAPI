using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hopaut.Modules.Posts.Infrastructure.Migrations
{
    /// <summary>
    /// Plan Step 8.2: the entrance fee becomes <c>Money</c> (amount numeric(10,2) + ISO-4217 code). Data conversion: the legacy
    /// currency index (Currencies.json at tag pre-net10-baseline; index 8 "BGL" becomes "BGN", D-026) turns into the code,
    /// a price without a currency defaults to EUR (index 0, the legacy default), NaN/infinite prices are dropped, and
    /// amounts are rounded to cents and clamped to 0 .. 99,999,999.99. Down converts back.
    /// </summary>
    public partial class EntranceFeeMoney : Migration
    {
        private const string LegacyCodes =
            "ARRAY['EUR','USD','CHF','GBP','TRY','UAH','SEK','RON','BGN','RUB','RSD','PLN','NOK','MDL','HUF','HRK','DKK','CZK','COP','AUD','CAD','MXN','BRL','BAM','ARS']";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "entrance_fee_amount",
                schema: "posts",
                table: "events",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "entrance_fee_currency",
                schema: "posts",
                table: "events",
                type: "character(3)",
                fixedLength: true,
                maxLength: 3,
                nullable: true);

            migrationBuilder.Sql($"""
                UPDATE posts.events
                SET entrance_fee_amount = LEAST(GREATEST(round("EntrancePrice"::numeric, 2), 0), 99999999.99),
                    entrance_fee_currency = COALESCE(({LegacyCodes})[COALESCE("Currency", 0) + 1], 'EUR')
                WHERE "EntrancePrice" IS NOT NULL
                  AND "EntrancePrice" NOT IN ('NaN'::float8, 'Infinity'::float8, '-Infinity'::float8);
                """);

            migrationBuilder.DropColumn(
                name: "EntrancePrice",
                schema: "posts",
                table: "events");

            migrationBuilder.DropColumn(
                name: "Currency",
                schema: "posts",
                table: "events");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "EntrancePrice",
                schema: "posts",
                table: "events",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Currency",
                schema: "posts",
                table: "events",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql($"""
                UPDATE posts.events
                SET "EntrancePrice" = entrance_fee_amount::float8,
                    "Currency" = array_position({LegacyCodes}, entrance_fee_currency::text) - 1
                WHERE entrance_fee_amount IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "entrance_fee_amount",
                schema: "posts",
                table: "events");

            migrationBuilder.DropColumn(
                name: "entrance_fee_currency",
                schema: "posts",
                table: "events");
        }
    }
}