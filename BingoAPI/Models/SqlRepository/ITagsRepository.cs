using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Models.SqlRepository
{
    public interface ITagsRepository
    {
        public Task<List<string>> FindTags(string tag);
    }
}
