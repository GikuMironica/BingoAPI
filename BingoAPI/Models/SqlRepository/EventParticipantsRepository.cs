using BingoAPI.Data;
using BingoAPI.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models.SqlRepository
{
    public class EventParticipantsRepository : IEventParticipantsRepository
    {
        private readonly DataContext context;
        private readonly UserManager<AppUser> userManager;

        public EventParticipantsRepository(DataContext context, UserManager<AppUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        public async Task<bool> AcceptAttendee(string userId, int postId)
        {
            var participation = await context.Participations
                .Where(p => p.PostId == postId && p.UserId == userId)
                .Include(p => p.Post)
                .ThenInclude(e => e.Event)
                .AsNoTracking()
                .SingleOrDefaultAsync();

            if (participation == null)
                return false;
            
            if(participation.Post.Event.GetSlotsIfAny() > 0)
            {
                var reserved = context.Participations.Where(p => p.PostId == postId && p.Accepted == 1).Count();
                if( participation.Post.Event.GetSlotsIfAny() > reserved)
                {
                    participation.Accepted = 1;
                    context.Participations.Update(participation);

                }
                else
                {
                    return false;
                }
            }
            else
            {
                participation.Accepted = 1;
                context.Participations.Update(participation);

            }
            await context.Database.BeginTransactionAsync();
            var result = await context.SaveChangesAsync() > 0;
            context.Database.CommitTransaction();
            return result;

        }

        public async Task<List<AppUser>> DisplayAll(int postId, PaginationFilter paginationFilter = null)
        {
            if (paginationFilter == null)
            {
                paginationFilter = new PaginationFilter { PageNumber = 1, PageSize = 50 };
            }

            var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;

            return await context.Participations
                .Where(p => p.PostId == postId)
                .Select(p => p.User)
                .AsNoTracking()
                .AsQueryable()
                .Skip(skip)
                .Take(paginationFilter.PageSize).ToListAsync();               
        }

        public Task<List<AppUser>> DisplayAllAccepted(int postId)
        {
            throw new NotImplementedException();
        }

        public Task<List<AppUser>> DisplayAllPending(int postId)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RejectAttendee(string userId, int postId)
        {
            var participation = await context.Participations
               .Where(p => p.PostId == postId && p.UserId == userId)
               .Include(p => p.Post)
               .ThenInclude(e => e.Event)
               .AsNoTracking()
               .SingleOrDefaultAsync();

            if (participation == null)
                return false;

            context.Participations.Remove(participation);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> IsPostOwnerAsync(int postId, string userId)
        {
            var post = await context.Posts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == postId);

            if (post == null)
            {
                return false;
            }
           
            return post.UserId == userId;
        }
    }
}
