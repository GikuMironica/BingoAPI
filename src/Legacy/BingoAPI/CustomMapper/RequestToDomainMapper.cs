using AutoMapper;
using Bingo.Contracts.V1.Requests.Post;
using BingoAPI.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.CustomMapper
{
    public class RequestToDomainMapper : IRequestToDomainMapper
    {
        public GetPostsFilter MapPostFilterRequestToDomain(IMapper mapper, FilteredGetAllPostsRequest filteredGetAllPosts)
        {
            var values = new List<bool>
            {
                filteredGetAllPosts.HouseParty != null && filteredGetAllPosts.HouseParty.Value,
                filteredGetAllPosts.Club != null && filteredGetAllPosts.Club.Value,
                filteredGetAllPosts.Bar != null && filteredGetAllPosts.Bar.Value,
                filteredGetAllPosts.CarMeet != null && filteredGetAllPosts.CarMeet.Value,
                filteredGetAllPosts.BicycleMeet != null && filteredGetAllPosts.BicycleMeet.Value,
                filteredGetAllPosts.BikerMeet != null && filteredGetAllPosts.BikerMeet.Value,
                filteredGetAllPosts.Marathon != null && filteredGetAllPosts.Marathon.Value,
                filteredGetAllPosts.Other != null && filteredGetAllPosts.Other.Value,
                filteredGetAllPosts.StreetParty != null && filteredGetAllPosts.StreetParty.Value
            };

            var count = values.Count(value => value);

            if (count == 0)
                return new GetPostsFilter
                {
                    HouseParty = true,
                    Bar = true,
                    Club = true,
                    StreetParty = true,
                    BicycleMeet = true,
                    BikerMeet = true,
                    CarMeet = true,
                    Other = true,
                    Marathon = true
                };
            else
                return mapper.Map<GetPostsFilter>(filteredGetAllPosts);
        }
    }
}
