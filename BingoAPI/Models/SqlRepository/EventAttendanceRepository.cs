using BingoAPI.Data;
using BingoAPI.Domain;
using BingoAPI.Models;
using BingoAPI.Models.SqlRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models.SqlRepository
{
    public class EventAttendanceRepository : IEventAttendanceRepository
    {
        private readonly IPostsRepository postsRepository;
        private readonly DataContext context;

        public EventAttendanceRepository(IPostsRepository postsRepository, DataContext context)
        {
            this.postsRepository = postsRepository;
            this.context = context;
        }
        public async Task<AttendedEventResult> AttendEvent(AppUser user, int postId)
        {
            var post = await postsRepository.GetByIdAsync(postId);
            if (post == null)
            {
                return new AttendedEventResult { Result = false };
            }

            var requested = await context.Participations.Where(p => p.PostId == postId && p.UserId == user.Id)
                .SingleOrDefaultAsync();

            if(requested != null)
            {
                return new AttendedEventResult { Result = false }; 
            }

            var attendedEventResult = new AttendedEventResult();

            string eventType = post.Event.GetType().Name.ToString();                        
            if (eventType == "HouseParty")
            {
                var accepted = await AttendHousePartyAsync(user, post);
                if (accepted)
                {
                    attendedEventResult.IsHouseParty = true;
                    attendedEventResult.HostId = post.UserId;
                }
                else
                {
                    return attendedEventResult;
                }                
            }
            else
            {
                await context.Participations.AddAsync(new Participation { Accepted = 1, Post = post, User = user });
            }

            await context.Database.BeginTransactionAsync();
            attendedEventResult.Result = await context.SaveChangesAsync() > 0;
            context.Database.CommitTransaction();

            return attendedEventResult;
        }

        private async Task<bool> AttendHousePartyAsync(AppUser user, Post post)
        {
            if (post.Participators == null)
            {
                post.Participators = new List<Participation>();
            }
            var reserved = post.Participators.Where(p => p.PostId == post.Id && p.Accepted == 1).Count();
            if (reserved < post.Event.GetSlotsIfAny())
            {
                await context.Participations.AddAsync(new Participation { Accepted = 0, Post = post, User = user });
                return true;
            }
            return false;
        }

        public async Task<List<Post>> GetActiveAttendedPostsByUserId(string userId)
        {
            return await context.Participations
                .Where(p => p.UserId == userId)
                .Include(p => p.Post)
                    .ThenInclude(p => p.Event)
                .Include(p => p.Post.Location)
                .Select(p => p.Post)
                .Where(p => p.ActiveFlag == 1)                
                .ToListAsync();
        }

        public async Task<List<Post>> GetOldAttendedPostsByUserId(string userId)
        {
            return await context.Participations
                .Where(p => p.UserId == userId)
                .Include(p => p.Post)
                    .ThenInclude(p => p.Event)
                .Include(p => p.Post.Location)
                .Select(p => p.Post)
                .Where(p => p.ActiveFlag == 0)
                .Take(50)
                .ToListAsync();
        }

        public async Task<bool> UnAttendEvent(AppUser user, int postId)
        {
            var Attendance = await context.Participations
                .Where(p => p.PostId == postId && p.UserId == user.Id)
                .SingleOrDefaultAsync();

            if (Attendance != null)
            {
                context.Participations.Remove(Attendance);
                await context.SaveChangesAsync();
            }
            return true;
        }
    }
}
