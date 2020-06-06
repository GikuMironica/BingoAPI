using Bingo.Contracts.V1.Requests.Post;
using BingoAPI.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

        public PostRepository(DataContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            this.userManager = userManager;
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

        public async Task<IEnumerable<Post>> GetAllAsync(Point location, int radius)
        {
            location.SRID = 4326;
            return await _context.Posts
                .Include(p => p.Location)
                .Include(p => p.Event)
                .Include(p => p.Repeatable)
                .Include(p => p.Voucher)
                .Where(p => p.ActiveFlag == 1 &&
                       p.Location.Location.IsWithinDistance(location, radius)).AsNoTracking().ToListAsync();
        }


        public async Task<bool> UpdateAsync(Post entity)
        {
            await AddNewTagsAsync(entity);
            _context.Posts.Update(entity);
            await _context.Database.BeginTransactionAsync();
                var updated = await _context.SaveChangesAsync();
            _context.Database.CommitTransaction();
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

            for(var i =0; i<post.Tags.Count; i++)
            {
                var tag = post.Tags[i];
                var existingTag = await _context.Tags.SingleOrDefaultAsync(x => x.TagName == tag.Tag.TagName);

                // if tag exists, increment counter
                if (existingTag != null)
                {
                    tag.Tag = existingTag;
                    continue;
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
                return true;
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
                if (dbUpdateEx.InnerException != null
                        && dbUpdateEx.InnerException.InnerException != null)
                {
                    if (dbUpdateEx.InnerException.InnerException is SqlException sqlException)
                    {
                        switch (sqlException.Number)
                        {
                            case 2627:  isUniqueConstraintFaulty = true; break;
                            case 547:   break;
                            case 2601:  break;
                            default: break;
                               
                        }
                    }                   
                }
            }
            return isUniqueConstraintFaulty;
        }

        public async Task<Post> GetPlainPost(int postId)
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
    }
}
