using Bingo.Contracts.V1.Requests.Post;
using BingoAPI.Data;
using BingoAPI.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models.SqlRepository
{
    public class PostRepository : IPostsRepository
    {
        protected readonly DataContext _context;
        private readonly UserManager<AppUser> userManager;
        private readonly EventTypes eventTypes;

        public PostRepository(DataContext context, UserManager<AppUser> userManager,
                              IOptions<EventTypes> eventTypes)
        {
            _context = context;
            this.userManager = userManager;
            this.eventTypes = eventTypes.Value;
        }

        public async Task<bool> AddAsync(Post entity)
        {
            var result = 0;
            bool tryAgain = true;
            entity.Voucher = new DrinkVoucher();
            entity.Repeatable = new RepeatableProperty();
            while (tryAgain)
            {
                try
                {
                    await AddNewTagsAsync(entity);
                    _context.Attach(entity.Event);
                    await _context.AddAsync(entity);
                    await _context.Database.BeginTransactionAsync();
                    result = await _context.SaveChangesAsync();
                    _context.Database.CommitTransaction();
                    tryAgain = false;
                }
                catch (Exception e)
                {
                    // if server tried to create a dublicate tag. try again
                    var isUniqueConstraintViolated = HandleInsertTagException(e);

                    // other exception which has to be logged
                    if (!isUniqueConstraintViolated)
                    {
                        tryAgain = false;

                    }
                }
            }
            return result > 0;

        }

        public async Task<bool> DeleteAsync(int Id)
        {
            var post = await _context.Posts.SingleOrDefaultAsync(p => p.Id == Id);

            if (post == null)
                return false;

            _context.Remove(post);
            
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<Post>> GetAllAsync()
        {
            return await _context.Posts.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetAllAsync(Point location, int radius, GetPostsFilter postsFilter, string Tag = "%", Int64 today = 0)
        {
            
            location.SRID = 4326;
            List<Post> posts = null;

            if (Tag.Equals("%"))
            {
                posts = await _context.Posts
                .Include(p => p.Location)
                .Include(p => p.Event)
                .Include(p => p.Repeatable)
                .Include(p => p.Voucher)
                .Include(p => p.Tags)
                .ThenInclude(pt => pt.Tag)
                .Where(p => p.ActiveFlag == 1 &&
                       p.Location.Location.IsWithinDistance(location, radius * 1000) &&
                       p.EventTime > today)
                .AsNoTracking().ToListAsync();
            }
            else
            {
                Tag = Tag.ToLower();
                posts = await _context.Posts
                .Include(p => p.Location)
                .Include(p => p.Event)
                .Include(p => p.Repeatable)
                .Include(p => p.Voucher)
                .Include(p => p.Tags)
                .ThenInclude(pt => pt.Tag)
                .Where(p => p.ActiveFlag == 1 &&
                       p.Location.Location.IsWithinDistance(location, radius * 1000) &&
                       p.EventTime > today &&
                       p.Tags.Count > 0 &&
                       p.Tags.All(pt => pt.Tag.TagName.Contains(Tag)))
                .AsNoTracking().ToListAsync();
            }          
           
            return FilterByType(posts, postsFilter);
        }

        private List<Post> FilterByType(List<Post> posts, GetPostsFilter postsFilter)
        {
            var filteredPosts = posts;
            if(posts.Count() > 0)
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
                    _context.Posts.Update(entity);
                    await _context.Database.BeginTransactionAsync();
                    updated = await _context.SaveChangesAsync();
                    _context.Database.CommitTransaction();
                    tryAgain = false;
                }
                catch (Exception e)
                {
                    // if server tried to create a dublicate tag. try again
                    var isUniqueConstraintViolated = HandleInsertTagException(e);

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
                var existingTag = await _context.Tags.SingleOrDefaultAsync(x => x.TagName == tag.Tag.TagName);

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
            var post = await _context.Posts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == postId);

            if (post == null)
            {
                return false;
            }
            var user = await userManager.FindByIdAsync(userId);

            var userRoles = await userManager.GetRolesAsync(user);

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
            return await _context.Posts
                .Include(p => p.Location)
                .Include(p => p.Event)
                .Include(p => p.Tags)
                    .ThenInclude(pt => pt.Tag)
                .Include(p => p.Participators)
                .Include(p => p.Voucher)
                .Include(p => p.Repeatable)
                .SingleOrDefaultAsync(x => x.Id == postId);
        }
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        

        private bool HandleInsertTagException(Exception exception)
        {
            var isUniqueConstraintFaulty = false;
            if (exception is DbUpdateException dbUpdateEx)
            {
                if (dbUpdateEx.InnerException != null)
                {
                    if (dbUpdateEx.InnerException.Message != null)
                    {
                        if (dbUpdateEx.InnerException.Message.Contains("23505"))
                        {
                            // inserted twice same tag
                            return false;
                        }
                        else
                        {
                            // Log smth else
                            return false;
                        }
                        
                    }
                }
                else
                {
                    // logg
                }
            }
            return isUniqueConstraintFaulty;
        }

        public async Task<Post> GetPlainPostAsync(int postId)
        {
            return await _context.Posts
                .Include(p => p.Event)
                .SingleOrDefaultAsync(x => x.Id == postId);
        }

        public async Task<List<string>> GetParticipantsIdAsync(int postId)
        {
            return await _context.Participations
                .Where(p => p.PostId ==  postId && p.Accepted == 1)
                .Select(p => p.UserId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> IsHostIdPostOwner(string hostId, int postId)
        {
            var result = await _context.Posts
                .Where(p => p.Id == postId && p.UserId == hostId)
                .CountAsync();

            return result > 0;
        }

        public async Task<string> GetHostId(int postId)
        {
            return await _context.Posts
                .Where(p => p.Id == postId)
                .Select(p => p.UserId)
                .SingleOrDefaultAsync();
        }

        public async Task<int> GetEventType(int postId)
        {
            var result = await _context.Events
                .Where(e => e.PostId == postId)
                .SingleOrDefaultAsync();

            string eventType = result.GetType().Name.ToString();
            return eventTypes.Types
                .Where(y => y.Type == eventType)
                .Select(x => x.Id)
                .FirstOrDefault();
        }

        public async Task<int> GetAvailableSlotsAsync(int postId)
        {
            var result = await _context.Participations
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

            return await _context.Posts
                .Where(p => p.UserId == userId && p.ActiveFlag == 1)
                .OrderByDescending(p => p.EventTime)
                .Include(p => p.Location)
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

            return await _context.Posts
               .Where(p => p.UserId == userId && p.ActiveFlag == 0)
               .OrderByDescending(p => p.EventTime)
               .Include(p => p.Location)
               .Include(p => p.Event)
               .Include(p => p.Voucher)
               .Include(p => p.Repeatable)
               .AsQueryable()
               .Skip(skip)
               .Take(paginationFilter.PageSize).ToListAsync();
        }
    }
}
