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
        public DbSet<Other> Others { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<PostTags> PostTags { get; set; }
        public DbSet<EventLocation> EventLocations { get; set; }
        public DbSet<Participation> Participations { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<UserReport> UserReports { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Rating> Rating { get; set; }
        public DbSet<RepeatableProperty> RepeatableProperties { get; set; }
        public DbSet<DrinkVoucher> DrinkVouchers { get; set; }
        public DbSet<UserVoucher> UserVouchers { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // - -
            modelBuilder.HasPostgresExtension("postgis");

            // configure primary key of PostTags
            modelBuilder.Entity<PostTags>()
                .HasKey(x => new { x.PostId, x.TagId });

            // configure primary key of Particiapants
            modelBuilder.Entity<Participation>()
                .HasKey(x => new {x.UserId, x.PostId });

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
                .HasValue<Other>("other_type");

            // indexes, constraints
            modelBuilder.Entity<Tag>()
                .HasIndex(t => t.TagName)
                .IsUnique(true);


            // One - to Many relationship between AppUser <-> Post
            modelBuilder.Entity<AppUser>()
                .HasMany(ap => ap.Posts)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // One - One relationship between Post <-> Event
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Post)
                .WithOne(p => p.Event)
                .HasForeignKey<Event>(p => p.PostId)
                .OnDelete(DeleteBehavior.Cascade);


            // One - One relationship between Post <-> Location
            modelBuilder.Entity<EventLocation>()
                .HasOne(l => l.Post)
                .WithOne(p => p.Location)
                .HasForeignKey<EventLocation>(p => p.PostId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Post>().Property(p => p.Pictures)
                .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());

            // Many - Many relationship Post <-> Tag
            modelBuilder.Entity<PostTags>()
                .HasOne(pt => pt.Tag)
                .WithMany(t => t.Posts)
                .HasForeignKey(pt => pt.TagId)
                .OnDelete(DeleteBehavior.NoAction); ;

            modelBuilder.Entity<PostTags>()
                .HasOne(pt => pt.Post)
                .WithMany(p => p.Tags)
                .HasForeignKey(pt => pt.PostId);


            // Many - Many between Post - Users 
            modelBuilder.Entity<Participation>()
                .HasOne(p => p.Post)
                .WithMany(p => p.Participators)
                .HasForeignKey(p => p.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Participation>()
                .HasOne(p => p.User)
                .WithMany(au => au.AttendedEvents)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // One - Many between Post - Announcements
            modelBuilder.Entity<Announcement>()
                .HasOne(a => a.Post)
                .WithMany(p => p.Announcements)
                .HasForeignKey(a => a.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many - One between Post - Reports
            modelBuilder.Entity<Report>()
                .HasOne(pr => pr.Post)
                .WithMany(p => p.Reports)
                .HasForeignKey(pr => pr.PostId)
                .OnDelete(DeleteBehavior.SetNull);

            // Many - One between Ratings - User ( Host )
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.User)
                .WithMany(au => au.Ratings)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // One - One between Post - Repeatable Property
            modelBuilder.Entity<RepeatableProperty>()
                .HasOne(r => r.Post)
                .WithOne(p => p.Repeatable)
                .HasForeignKey<RepeatableProperty>(r => r.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // One - One between post - Voucher
            modelBuilder.Entity<DrinkVoucher>()
                 .HasOne(dv => dv.Post)
                 .WithOne(p => p.Voucher)
                 .HasForeignKey<DrinkVoucher>(dv => dv.PostId)
                 .OnDelete(DeleteBehavior.Cascade);

            // One - Many between DrinkVoucher - Users
            modelBuilder.Entity<UserVoucher>()
                .HasOne(uv => uv.DrinkVoucher)
                .WithMany(dv => dv.UserVouchers)
                .HasForeignKey(uv => uv.DrinkVoucherId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserVoucher>()
                .HasOne(uv => uv.User)
                .WithMany(au => au.Vouchers)
                .HasForeignKey(uv => uv.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            // One - Many between User -> UserReports
            modelBuilder.Entity<UserReport>()
                .HasOne(r => r.ReportedUser)
                .WithMany(u => u.Reports)
                .HasForeignKey(r => r.ReportedUserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        

    }
}
