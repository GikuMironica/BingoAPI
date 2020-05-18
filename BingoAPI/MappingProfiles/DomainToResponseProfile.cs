using AutoMapper;
using Bingo.Contracts.V1.Requests.Post;
using Bingo.Contracts.V1.Responses.Post;
using Bingo.Contracts.V1.Responses.User;
using BingoAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.MappingProfiles
{
    public class DomainToResponseProfile : Profile
    {
        public DomainToResponseProfile()
        {
            CreateMap<AppUser, UserResponse>();

            CreateMap<Post, CreatePostResponse>()
                .ForMember(dest => dest.Tags, opt =>
                    opt.MapFrom(src => src.Tags.Select(x => x.Tag.TagName)));
            
        }
        
    }
}
