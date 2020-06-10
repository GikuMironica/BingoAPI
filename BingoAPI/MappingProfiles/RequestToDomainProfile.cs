using AutoMapper;
using Bingo.Contracts.V1.Requests.Announcement;
using Bingo.Contracts.V1.Requests.Post;
using Bingo.Contracts.V1.Requests.Rating;
using Bingo.Contracts.V1.Requests.User;
using BingoAPI.Domain;
using BingoAPI.Models;
using NetTopologySuite.Geometries;
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

            CreateMap<PaginationQuery, PaginationFilter>();

            // Map child of UpdatePostRequest to Location
            CreateMap<UpdatedCompleteLocation, EventLocation>()
                .ForMember(dest => dest.Location, opt => opt.MapFrom(src => new Point(src.Longitude.Value, src.Latitude.Value)))
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
                .ForMember(x => x.EventTime, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            // .ForMember(x => x.EventTime, opt => opt.Condition(s => s.EventTime.HasValue && s.EventTime != 0 && s.EventTime != null))


            // Map CreateAnnouncement request to Model, for update too
            CreateMap<CreateAnnouncementRequest, Announcement>();
            CreateMap<UpdateAnnouncementRequest, Announcement>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));


            // Map Ratings
            CreateMap<CreateRatingRequest, Rating>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
        }
    }

   }
