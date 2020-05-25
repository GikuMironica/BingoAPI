using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public interface IUriService
    {

        public Uri GetPostUri(string postId);
        
    }
}
