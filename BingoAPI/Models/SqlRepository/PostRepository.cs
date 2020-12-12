using BingoAPI.Data;
using BingoAPI.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BingoAPI.Options;
using BingoAPI.Services;

namespace BingoAPI.Models.SqlRepository
{
    public class PostRepository : IPostsRepository
    {
        protected readonly DataContext Context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IErrorService _errorService;
        private readonly EventTypes _eventTypes;

        public PostRepository(DataContext context, UserManager<AppUser> userManager,
                              IOptions<EventTypes> eventTypes, IErrorService errorService)
        {
            Context = context;
            this._userManager = userManager;
            _errorService = errorService;
            this._eventTypes = eventTypes.Value;
        }


        
        public async Task<bool> AddAsync(Post entity)
        {
            var result = 0;
            var tryAgain = true;
            entity.Voucher = new DrinkVoucher();
            entity.Repeatable = new RepeatableProperty();
            while (tryAgain)
            {
                try
                {
                    await AddNewTagsAsync(entity);
                    Context.Attach(entity.Event);
                    await Context.AddAsync(entity);
                    await Context.Database.BeginTransactionAsync();
                    result = await Context.SaveChangesAsync();
                    Context.Database.CommitTransaction();
                    tryAgain = false;
                }
                catch (Exception e)
                {
                    // if server tried to create a duplicate tag. try again
                    var isUniqueConstraintViolated = await HandleInsertTagException(e);

                    // other exception which has to be logged
                    if (!isUniqueConstraintViolated)
                    {
                        tryAgain = false;
                    }
                }
            }
            return result > 0;

        }

        public async Task<bool> DeleteAsync(int id)
        {
            var post = await Context.Posts.SingleOrDefaultAsync(p => p.Id == id);

            if (post == null)
                return false;

            Context.Remove(post);
            
            return await Context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Post>> GetAllAsync()
        {
            return await Context.Posts.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetAllAsync(Point location, int radius, GetPostsFilter postsFilter, Int64 today, string tag = "%")
        {
            
            location.SRID = 4326;
            List<Post> posts;

            if (tag.Equals("%"))
            {
                posts = await Context.Posts
                .Include(p => p.Location)
                .Include(p => p.Event)
                .Include(p => p.Pictures)
                .Include(p => p.Repeatable)
                .Include(p => p.Voucher)
                .Include(p => p.Tags)
                .ThenInclude(pt => pt.Tag)
                .Where(p => p.ActiveFlag == 1 &&
                       p.Location.Location.IsWithinDistance(location, radius * 1000) &&
                       p.EndTime < today)
                .AsNoTracking().ToListAsync();
            }
            else
            {
                tag = tag.ToLower();
                posts = await Context.Posts
                .Include(p => p.Location)
                .Include(p => p.Event)
                .Include(p => p.Pictures)
                .Include(p => p.Repeatable)
                .Include(p => p.Voucher)
                .Include(p => p.Tags)
                .ThenInclude(pt => pt.Tag)
                .Where(p => p.ActiveFlag == 1 &&
                       p.Location.Location.IsWithinDistance(location, radius * 1000) &&
                       p.EndTime < today &&
                       p.Tags.Count > 0 &&
                       p.Tags.Any(pt => pt.Tag.TagName.Contains(tag)))
                .AsNoTracking().ToListAsync();
            }          
           
            return FilterByType(posts, postsFilter);
        }

        private List<Post> FilterByType(List<Post> posts, GetPostsFilter postsFilter)
        {
            var filteredPosts = posts;
            if(posts.Any())
            {
                if (postsFilter.HouseParty == false)
                    filteredPosts.RemoveAll(p =>p.Event.GetType().Name.ToString() == "HouseParty");
                if (postsFilter.Bar == false)
                    filteredPosts.RemoveAll(p => p.Event.GetType().Name.ToString() == "Bar");
                if (postsFilter.Club == false)
                    filteredPosts.RemoveAll(p => p.Event.GetType().Name.ToString() == "Club");
                if (postsFilter.CarMeet == false)
                    filteredPosts.RemoveAll(p => p.Event.GetType().Name.ToString() == "CarMeet");
                if (postsFilter.BikerMeet == false)
                    filteredPosts.RemoveAll(p => p.Event.GetType().Name.ToString() == "BikerMeet");
                if (postsFilter.BicycleMeet == false)
                    filteredPosts.RemoveAll(p => p.Event.GetType().Name.ToString() == "BicycleMeet");
                if (postsFilter.StreetParty == false)
                    filteredPosts.RemoveAll(p => p.Event.GetType().Name.ToString() == "StreetParty");
                if (postsFilter.Other == false)
                    filteredPosts.RemoveAll(p => p.Event.GetType().Name.ToString() == "Other");
                if (postsFilter.Marathon == false)
                    filteredPosts.RemoveAll(p => p.Event.GetType().Name.ToString() == "Marathon");
            }
            return filteredPosts;
        }

        public async Task<bool> UpdateAsync(Post entity)
        {
            int updated = 0;
            bool tryAgain = true;
            while (tryAgain)
            {
                try
                {
                    await AddNewTagsAsync(entity);
                    Context.Posts.Update(entity);
                    await Context.Database.BeginTransactionAsync();
                    updated = await Context.SaveChangesAsync();
                    Context.Database.CommitTransaction();
                    tryAgain = false;
                }
                catch (Exception e)
                {
                    // if server tried to create a duplicate tag. try again
                    var isUniqueConstraintViolated = await HandleInsertTagException(e);

                    // other exception which has to be logged
                    if (!isUniqueConstraintViolated)
                    {
                        tryAgain = false;

                    }
                }
            }
           
            return updated > 0; 
        }

        /// <summary>
        /// This method checks if tag already exit or not,
        /// if exists increment counter
        /// if not, add it, initialize counter
        /// </summary>
        /// <param name="post">The post which references the tag</param>
        /// <returns></returns>
        public async Task AddNewTagsAsync(Post post)
        {
            // store tags name in lower case
            if ((post.Tags == null) || (post.Tags.Count==0))
                return;
            post.Tags?.ForEach(pt => pt.Tag.TagName = pt.Tag.TagName.ToLower());

            for(var i=0; i<post.Tags.Count; i++)
            {
                var tag = post.Tags[i];
                var existingTag = await Context.Tags.SingleOrDefaultAsync(x => x.TagName == tag.Tag.TagName);

                // if tag exists, link to it
                if (existingTag != null)
                {
                    tag.Tag = existingTag;
                }

                  // else
                  //await _context.Tags.AddAsync(new Tag { Counter = 1, TagName = tag.Tag.TagName, Posts = post.Tags });
            }
        }

        public async Task<bool> IsPostOwnerOrAdminAsync(int postId, string userId)
        {
            var post = await Context.Posts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == postId);

            if (post == null)
            {
                return false;
            }
            var user = await _userManager.FindByIdAsync(userId);

            var userRoles = await _userManager.GetRolesAsync(user);

            var isAdmin = false;

            foreach(var role in userRoles)
            {
                if (role == "Admin" || role == "SuperAdmin")
                    isAdmin = true;
            }


            return post.UserId == userId || isAdmin;
        }

        public async Task<Post> GetByIdAsync(int postId)
        {
            return await Context.Posts
                .Include(p => p.Pictures)
                .Include(p => p.Location)
                .Include(p => p.Event)
                .Include(p => p.Tags)
                    .ThenInclude(pt => pt.Tag)
                .Include(p => p.Participators)
                .Include(p => p.Voucher)
                .Include(p => p.Repeatable)
                .SingleOrDefaultAsync(x => x.Id == postId);
        }
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        

        private async Task<bool> HandleInsertTagException(Exception exception)
        {
            if (exception is DbUpdateException dbUpdateEx)
            {
                if (dbUpdateEx.InnerException?.Message != null)
                {
                    if (dbUpdateEx.InnerException.Message.Contains("23505"))
                    {
                        // inserted twice same tag
                        return false;
                    }

                    await LogError(dbUpdateEx.InnerException.Message, dbUpdateEx.InnerException.StackTrace);
                    return false;
                }
            }
            await LogError(exception.Message+" \n Notice: Most probably someone uses PostMan to test the endpoint.", exception.StackTrace);
            return false;
        }



        private async Task LogError(string message, string stacktrace)
        {
            var log = new ErrorLog
            {
                Message = message,
                ExtraData = stacktrace,
                Date = DateTime.Now
            };
            await _errorService.AddErrorAsync(log);
        }



        public async Task<Post> GetPlainPostAsync(int postId)
        {
            return await Context.Posts
                .Include(p => p.Pictures)
                .Include(p => p.Event)
                .SingleOrDefaultAsync(x => x.Id == postId);
        }

        public async Task<List<string>> GetParticipantsIdAsync(int postId)
        {
            return await Context.Participations
                .Where(p => p.PostId ==  postId && p.Accepted == 1)
                .Select(p => p.UserId)
                .AsNoTracking()
                .ToListAsync();
        }



        public async Task<bool> IsHostIdPostOwner(string hostId, int postId)
        {
            var result = await Context.Posts
                .Where(p => p.Id == postId && p.UserId == hostId)
                .CountAsync();

            return result > 0;
        }



        public async Task<string> GetHostId(int postId)
        {
            return await Context.Posts
                .Where(p => p.Id == postId)
                .Select(p => p.UserId)
                .SingleOrDefaultAsync();
        }



        public async Task<int> GetEventType(int postId)
        {
            var result = await Context.Events
                .Where(e => e.PostId == postId)
                .SingleOrDefaultAsync();

            string eventType = result.GetType().Name;
            return _eventTypes.Types
                .Where(y => y.Type == eventType)
                .Select(x => x.Id)
                .FirstOrDefault();
        }



        public async Task<int> GetAvailableSlotsAsync(int postId)
        {
            var result = await Context.Participations
                .Where(p => p.PostId == postId && p.Accepted == 1)
                .CountAsync();

            var total = await GetPlainPostAsync(postId);
            return total.Event.GetSlotsIfAny() - result;
        }



        public async Task<IEnumerable<Post>> GetMyActive(string userId, PaginationFilter paginationFilter)
        {
            if (paginationFilter == null)
            {
                paginationFilter = new PaginationFilter { PageNumber = 1, PageSize = 50 };
            }

            var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;

            return await Context.Posts
                .Where(p => p.UserId == userId && p.ActiveFlag == 1)
                .OrderByDescending(p => p.EventTime)
                .Include(p => p.Location)
                .Include(p => p.Pictures)
                .Include(p => p.Event)
                .Include(p => p.Voucher)
                .Include(p => p.Repeatable)
                .AsQueryable()
                .Skip(skip)
                .Take(paginationFilter.PageSize).ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetMyInactive(string userId, PaginationFilter paginationFilter)
        {
            if (paginationFilter == null)
            {
                paginationFilter = new PaginationFilter { PageNumber = 1, PageSize = 50 };
            }

            var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;

            return await Context.Posts
               .Where(p => p.UserId == userId && p.ActiveFlag == 0)
               .OrderByDescending(p => p.EventTime)
               .Include(p => p.Location)
               .Include(p => p.Pictures)
               .Include(p => p.Event)
               .Include(p => p.Voucher)
               .Include(p => p.Repeatable)
               .AsQueryable()
               .Skip(skip)
               .Take(paginationFilter.PageSize).ToListAsync();
        }

        public async Task<bool> DisablePost(Post post)
        {
            post.ActiveFlag = 0;
            Context.Posts.Update(post);
            var updated = await Context.SaveChangesAsync();
            return updated > 0;
        }

        public async Task<int> GetActiveEventsNumbers(string userId)
        {
            return await Context.Posts
                .Where(p => p.UserId == userId
                         && p.ActiveFlag == 1)
                .CountAsync();
        }
    }
}
