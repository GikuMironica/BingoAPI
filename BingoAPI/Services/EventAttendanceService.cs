using BingoAPI.Data;
using BingoAPI.Models;
using BingoAPI.Models.SqlRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public class EventAttendanceService : IEventAttendanceService
    {
        private readonly IPostsRepository postsRepository;
        private readonly DataContext context;

        public EventAttendanceService(IPostsRepository postsRepository, DataContext context)
        {
            this.postsRepository = postsRepository;
            this.context = context;
        }
        public async Task<bool> AttendEvent(AppUser user, int postId)
        {
            var post = await postsRepository.GetByIdAsync(postId);
            if (post == null)
            {
                return false;
            }

            string eventType = post.Event.GetType().Name.ToString();
            
            if (eventType == "HouseParty")
            {
                AttendHouseParty(user, post);
            }
            else
            {

            }

            await context.Database.BeginTransactionAsync();
            var result = await context.SaveChangesAsync();
            context.Database.CommitTransaction();

            return result > 0;
        }

        private async void AttendHouseParty(AppUser user, Post post)
        {
            if (post.Participators == null)
            {
                post.Participators = new List<Participation>();
            }
            var reserved = post.Participators.Where(p => p.PostId == post.Id && p.Accepted == 1).Count();
            if (reserved < post.Event.GetSlotsIfAny())
            {
                await context.Participations.AddAsync(new Participation { Accepted = 0, Post = post, User = user });
            }                   
        }

        public async Task<List<Post>> GetActiveAttendedPostsByUserId(string userId)
        {
            return await context.Participations
                .Where(p => p.UserId == userId)
                .Select(p => p.Post)
                .Where(p => p.ActiveFlag == 1)
                .Include(p => p.Event)
                .ToListAsync();
        }

        public async Task<List<Post>> GetOldAttendedPostsByUserId(string userId)
        {
            return await context.Participations
                .Where(p => p.UserId == userId)
                .Select(p => p.Post)
                .Where(p => p.ActiveFlag == 0)
                .Include(p => p.Event)
                .ToListAsync();
        }

        public Task<bool> UnAttendEvent(AppUser user, int postId)
        {
            throw new NotImplementedException();
        }
    }
}
