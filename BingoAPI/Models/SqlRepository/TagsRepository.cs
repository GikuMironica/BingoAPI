using BingoAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models.SqlRepository
{
    public class TagsRepository : ITagsRepository
    {
        private readonly DataContext _context;

        public TagsRepository(DataContext context)
        {
            this._context = context;
        }

        public async Task<List<string>> FindTags(string tag)
        {
            var lowerTag = tag.ToLower();
            return await _context.Tags
                .Where(p => p.TagName.StartsWith(lowerTag))
                .Select(p => p.TagName)
                .Take(10)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
