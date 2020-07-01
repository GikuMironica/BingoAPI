using AutoMapper;
using Bingo.Contracts.V1.Requests.Post;
using BingoAPI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.CustomMapper
{
    public interface IRequestToDomainMapper
    {
        public GetPostsFilter MapPostFilterRequestToDomain(IMapper mapper, FilteredGetAllPostsRequest filteredGetAllPosts);
    }
}
