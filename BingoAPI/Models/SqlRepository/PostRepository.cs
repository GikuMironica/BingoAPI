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

        public async Task<bool> Add(Post entity)
        {
            await AddNewTags(entity);
            await _context.AddAsync(entity);
            var result = await _context.SaveChangesAsync();
                return result > 0;
        }

        public Task Delete(int Id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Post>> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<Post> GetById(int id)
        {
            throw new NotImplementedException();
        }

        public Task Update(Post entity)
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
        public async Task AddNewTags(Post post)
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
