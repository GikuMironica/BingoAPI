using BingoAPI.Data;
using BingoAPI.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models.SqlRepository
{
    public class EventParticipantsRepository : IEventParticipantsRepository
    {
        private readonly DataContext _context;
        private readonly UserManager<AppUser> _userManager;

        public EventParticipantsRepository(DataContext context, UserManager<AppUser> userManager)
        {
            this._context = context;
            this._userManager = userManager;
        }

        public async Task<ProcessAttendRequest> AcceptAttendee(string userId, int postId)
        {
            var participation = await _context.Participations
                .Where(p => p.PostId == postId && p.UserId == userId)
                .Include(p => p.Post)
                .ThenInclude(e => e.Event)
                .AsNoTracking()
                .SingleOrDefaultAsync();

            if (participation == null)
                return new ProcessAttendRequest { Result = false};
            if(participation.Accepted == 1)
            {
                return new ProcessAttendRequest { Result = false };
            }
            
            if(participation.Post.Event.GetSlotsIfAny() > 0)
            {
                var reserved = _context.Participations.Count(p => p.PostId == postId && p.Accepted == 1);
                if( participation.Post.Event.GetSlotsIfAny() > reserved)
                {
                    participation.Accepted = 1;
                    _context.Participations.Update(participation);

                }
                else
                {
                    return new ProcessAttendRequest { Result = false};
                }
            }
            else
            {
                participation.Accepted = 1;
                _context.Participations.Update(participation);

            }
            await _context.Database.BeginTransactionAsync();
            var resultObject = new ProcessAttendRequest
            {
                Result = await _context.SaveChangesAsync() > 0,
                EventTitle = participation.Post.Event.Title
            };
            await _context.Database.CommitTransactionAsync();            
        
            return resultObject;
        }

        public async Task<List<AppUser>> DisplayAll(int postId, PaginationFilter paginationFilter = null)
        {
            paginationFilter ??= new PaginationFilter {PageNumber = 1, PageSize = 50};

            var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;

            return await _context.Participations
                .Where(p => p.PostId == postId)
                .Select(p => p.User)
                .AsNoTracking()
                .AsQueryable()
                .Skip(skip)
                .Take(paginationFilter.PageSize).ToListAsync();               
        }

        public async Task<List<AppUser>> DisplayAllAccepted(int postId, PaginationFilter paginationFilter = null)
        {
            paginationFilter ??= new PaginationFilter {PageNumber = 1, PageSize = 50};

            var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;

            return await _context.Participations
                .Where(p => p.PostId == postId && p.Accepted == 1)
                .Select(p => p.User)
                .AsNoTracking()
                .AsQueryable()
                .Skip(skip)
                .Take(paginationFilter.PageSize).ToListAsync();
        }

        public async Task<List<AppUser>> DisplayShortlyAccepted(int postId)
        {
            return await _context.Participations
                .Where(p => p.PostId == postId && p.Accepted == 1)
                .Select(p => p.User)
                .AsNoTracking()
                .Take(3)
                .ToListAsync();
        }

        public async Task<int> CountAccepted(int postId)
        {
            return await _context.Participations
                .Where(p => p.PostId == postId && p.Accepted == 1)
                .AsNoTracking()
                .CountAsync();
        }


        public async Task<List<AppUser>> DisplayAllPending(int postId, PaginationFilter paginationFilter = null)
        {
            paginationFilter ??= new PaginationFilter {PageNumber = 1, PageSize = 50};

            var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;

            return await _context.Participations
                .Where(p => p.PostId == postId && p.Accepted == 0)
                .Select(p => p.User)
                .AsNoTracking()
                .AsQueryable()
                .Skip(skip)
                .Take(paginationFilter.PageSize).ToListAsync();
        }

        public async Task<bool> RejectAttendee(string userId, int postId)
        {
            var participation = await _context.Participations
               .Where(p => p.PostId == postId && p.UserId == userId)
               .Include(p => p.Post)
               .ThenInclude(e => e.Event)
               .AsNoTracking()
               .SingleOrDefaultAsync();

            if (participation == null)
                return false;

            _context.Participations.Remove(participation);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> IsPostOwnerAsync(int postId, string userId)
        {
            // TODO - to refactor
            var post = await _context.Posts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == postId);

            if (post == null)
            {
                return false;
            }
           
            return post.UserId == userId;
        }

        public async Task<bool> IsParticipatorAsync(int postId, string userId)
        {
            var count = await _context.Participations
                .Where(p => p.PostId == postId && p.UserId == userId && p.Accepted == 1)
                .AsNoTracking()
                .CountAsync();

            return count > 0;
        }
    }
}
