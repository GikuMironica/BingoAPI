using BingoAPI.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models.SqlRepository
{
    public class TagsRepository : ITagsRepository
    {
        private readonly DataContext context;

        public TagsRepository(DataContext context)
        {
            this.context = context;
        }

        public async Task<List<string>> FindTags(string tag)
        {
            string lowerTag = tag.ToLower();
            return await context.Tags
                .Where(p => p.TagName.StartsWith(lowerTag))
                .Select(p => p.TagName)
                .Take(10)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
