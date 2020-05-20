using AutoMapper;
using Bingo.Contracts.V1.Requests.Post;
using Bingo.Contracts.V1.Requests.User;
using BingoAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.MappingProfiles
{
    public class RequestToDomainProfile : Profile
    {
        public RequestToDomainProfile()
        {
            CreateMap<UpdateUserRequest, AppUser>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            
            // Map child of UpdatePostRequest to Location
            CreateMap<UpdatedCompleteLocation, Location>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Map child of UpdatePostRequest to Event
            CreateMap<UpdatedEvent, Event>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // Map the whole UpdatePostRequest with Post including the child objects mapping profiles
            CreateMap<UpdatePostRequest, Post>()
                .ForMember(p => p.Location,
                           opt => opt.MapFrom(s => s.UserLocation))
                .ForMember(p => p.Event,
                           opt => opt.MapFrom(s => s.Event))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
                
        }
    }

   }
