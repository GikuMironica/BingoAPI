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
            List<bool> values = new List<bool>
            {
                filteredGetAllPosts.HouseParty.Value,
                filteredGetAllPosts.Club.Value,
                filteredGetAllPosts.Bar.Value,
                filteredGetAllPosts.CarMeet.Value,
                filteredGetAllPosts.BicycleMeet.Value,
                filteredGetAllPosts.BikerMeet.Value,
                filteredGetAllPosts.Marathon.Value,
                filteredGetAllPosts.Other.Value,
                filteredGetAllPosts.StreetParty.Value
            };

            int count = 0;
            foreach(var value in values)
            {
                if (value == true)
                    count++;
            }

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
