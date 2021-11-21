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
        private readonly IPostsRepository _postsRepository;
        private readonly DataContext _context;

        public EventAttendanceRepository(IPostsRepository postsRepository, DataContext context)
        {
            this._postsRepository = postsRepository;
            this._context = context;
        }

        public async Task<AttendedEventResult> AttendEvent(AppUser user, int postId)
        {
            var post = await _postsRepository.GetByIdAsync(postId);
            if (post == null)
            {
                return new AttendedEventResult { Result = false };
            }
            // if event in past
            if(post.EndTime < DateTimeOffset.UtcNow.ToLocalTime().ToUnixTimeSeconds())
            {
                return new AttendedEventResult { Result = false };
            }

            var requested = await _context.Participations.
                Where(p => p.PostId == postId && p.UserId == user.Id)
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
                var participation = new Participation {Accepted = 1, Post = post, User = user};
                await _context.Participations.AddAsync(participation);
                /*
                _context.Entry(participation.User).State = EntityState.Unchanged;
                _context.Entry(participation.Post).State = EntityState.Unchanged;*/
            }

            await _context.Database.BeginTransactionAsync();
            attendedEventResult.Result = await _context.SaveChangesAsync() > 0;
            await _context.Database.CommitTransactionAsync();

            return attendedEventResult;
        }

        private async Task<bool> AttendHousePartyAsync(AppUser user, Post post)
        {
            post.Participators ??= new List<Participation>();
            var reserved = post.Participators.Count(p => p.PostId == post.Id && p.Accepted == 1);
            if (reserved >= post.Event.GetSlotsIfAny()) return false;
            await _context.Participations.AddAsync(new Participation { Accepted = 0, Post = post, User = user });
            return true;
        }

        public async Task<List<Post>> GetActiveAttendedPostsByUserId(string userId)
        {
            var now = DateTimeOffset.UtcNow.ToLocalTime().ToUnixTimeSeconds();
            return await _context.Participations
                .Where(p => p.UserId == userId && p.Accepted == 1 )
                .Include(p => p.Post)
                .Include(p => p.Post.Location)
                .Include(p => p.Post.Event)
                .Include(p => p.Post.Repeatable)
                .Include(p => p.Post.Tags)
                    .ThenInclude(pt => pt.Tag)
                .Select(p => p.Post)
                .Where(p => p.ActiveFlag == 1 && p.EndTime > now)                
                .ToListAsync();
        }

        public async Task<List<Post>> GetOldAttendedPostsByUserId(string userId)
        {
            var now = DateTimeOffset.UtcNow.ToLocalTime().ToUnixTimeSeconds();
            return await _context.Participations
                .Where(p => p.UserId == userId && p.Accepted == 1)
                .Include(p => p.Post)
                .Include(p => p.Post.Location)
                .Include(p => p.Post.Event)
                .Include(p => p.Post.Repeatable)
                .Include(p => p.Post.Tags)
                    .ThenInclude(pt => pt.Tag)
                .Select(p => p.Post)
                .Where(p => p.ActiveFlag == 0 || p.EndTime < now)
                .Take(30)
                .ToListAsync();
        }

        public async Task<bool> UnAttendEvent(AppUser user, int postId)
        {
            var attendance = await _context.Participations
                .Where(p => p.PostId == postId && p.UserId == user.Id)
                .SingleOrDefaultAsync();

            if (attendance == null) return true;
            _context.Participations.Remove(attendance);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> IsUserAttendingEvent(string userId, int postId)
        {
            var result = await _context.Participations
                    .Where(p => p.UserId == userId && p.PostId == postId && p.Accepted == 1)
                    .CountAsync();

            return result > 0;
        }

        
    }
}
