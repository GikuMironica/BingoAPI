using BingoAPI.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models.SqlRepository
{
    public class PostRepository : IPostsRepository
    {
        protected readonly DataContext _context;

        // datacontext will be injected by DI
        public PostRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> AddAsync(Post entity)
        {
            await AddNewTagsAsync(entity);
            await _context.AddAsync(entity);
            var result = await _context.SaveChangesAsync();
                return result > 0;
        }

        public Task DeleteAsync(int Id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Post>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Post> GetByIdAsync(int id)
        {
            return await _context.Posts
                .Include(tag => tag.Location)
                .Include(tag => tag.Event)
                .Include(tag => tag.Tags)
                    .ThenInclude(pt => pt.Tag)
                .SingleOrDefaultAsync(x => x.Id == id);
        }

        public Task UpdateAsync(Post entity)
        {
            throw new NotImplementedException();
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
                    existingTag.Counter++;
                    tag.Tag = existingTag;
                    continue;
                }

                  // else
                  //await _context.Tags.AddAsync(new Tag { Counter = 1, TagName = tag.Tag.TagName, Posts = post.Tags });
            }
        }
    }
}
