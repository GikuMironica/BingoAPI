using Bingo.Contracts.V1.Requests.Post;
using BingoAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.CustomMapper
{
    public interface IUpdatePostToDomain
    {
        public Post Map(UpdatePostRequest updatePostRequest, Post post);
    }
}
