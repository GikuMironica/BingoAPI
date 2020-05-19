using BingoAPI.Domain;
using BingoAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace BingoAPI.Data

{
    public class DataContext : IdentityDbContext
    {              

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<HouseParty> HouseParties { get; set; }
        public DbSet<Bar> Bars { get; set; }
        public DbSet<Club> Clubs { get; set; }
        public DbSet<CarMeet> CarMeets { get; set; }
        public DbSet<BicycleMeet> BicycleMeets { get; set; }
        public DbSet<BikerMeet> BikerMeets { get; set; }
        public DbSet<Marathon> Marathons { get; set; }
        public DbSet<StreetParty> StreetParties { get; set; }
        public DbSet<FlashMob> FlashMobs { get; set; }
        public DbSet<Other> Others { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<PostTags> PostTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // configure primary key of PostTags
            modelBuilder.Entity<PostTags>()
                .HasKey(x => new { x.PostId, x.TagId });

            // Define the TPH using Fluent.API
            modelBuilder.Entity<Event>()
                .ToTable("Events")
                .HasDiscriminator<string>("event_type")
                .HasValue<HouseParty>("house_type")
                .HasValue<Bar>("bar_type")
                .HasValue<Club>("club_type")
                .HasValue<CarMeet>("carmeet_type")
                .HasValue<BikerMeet>("bikermeet_type")
                .HasValue<BicycleMeet>("bicyclemeet_type")
                .HasValue<Marathon>("marathon_type")
                .HasValue<StreetParty>("streetparty_type")
                .HasValue<FlashMob>("flashmob_type")
                .HasValue<Other>("other_type");


            // One - to Many relationship between AppUser <-> Post
            modelBuilder.Entity<AppUser>()
                .HasMany(ap => ap.Posts)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // One - One relationship between Post <-> Event
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Event)
                .WithOne(e => e.Post)
                .HasForeignKey<Event>(p => p.Id)
                .OnDelete(DeleteBehavior.Cascade);


            // One - One relationship between Post <-> Location
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Location)
                .WithOne(l => l.Post)
                .HasForeignKey<Location>(p => p.Id)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Post>().Property(p => p.Pictures)
                .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));

            // One - Many relationship Post <-> Tag
            modelBuilder.Entity<PostTags>()
                .HasOne(pt => pt.Tag)
                .WithMany(t => t.Posts)
                .HasForeignKey(pt => pt.TagId)
                .OnDelete(DeleteBehavior.NoAction); ;

            modelBuilder.Entity<PostTags>()
                .HasOne(pt => pt.Post)
                .WithMany(p => p.Tags)
                .HasForeignKey(pt => pt.PostId);
        }

        

    }
}
