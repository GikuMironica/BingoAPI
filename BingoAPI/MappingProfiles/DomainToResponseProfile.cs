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

            CreateMap<Post, PostResponse>()
                .ForPath(dest => dest.Location.Id, opt => opt.MapFrom(src => src.Location.Id))
                .ForPath(dest => dest.Location.Latitude, opt => opt.MapFrom(src => src.Location.Latitude))
                .ForPath(dest => dest.Location.Logitude, opt => opt.MapFrom(src => src.Location.Logitude))
                .ForPath(dest => dest.Location.Region, opt => opt.MapFrom(src => src.Location.Region))
                .ForPath(dest => dest.Location.Address, opt => opt.MapFrom(src => src.Location.Address))
                .ForPath(dest => dest.Location.City, opt => opt.MapFrom(src => src.Location.City))
                .ForPath(dest => dest.Location.Country, opt => opt.MapFrom(src => src.Location.Country))
                .ForPath(dest => dest.Event.Id, opt => opt.MapFrom(src => src.Event.Id))
                .ForPath(dest => dest.Event.Description, opt => opt.MapFrom(src => src.Event.Description))
                .ForPath(dest => dest.Event.Requirements, opt => opt.MapFrom(src => src.Event.Requirements))
                .ForPath(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.Select(x => x.Tag.TagName)));







        }
        
    }
}
