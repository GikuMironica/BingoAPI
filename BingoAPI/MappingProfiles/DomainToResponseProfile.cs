using AutoMapper;
using Bingo.Contracts.V1.Requests.Post;
using Bingo.Contracts.V1.Responses.Announcement;
using Bingo.Contracts.V1.Responses.AttendedEvent;
using Bingo.Contracts.V1.Responses.EventAttendee;
using Bingo.Contracts.V1.Responses.Post;
using Bingo.Contracts.V1.Responses.Profile;
using Bingo.Contracts.V1.Responses.Rating;
using Bingo.Contracts.V1.Responses.Report;
using Bingo.Contracts.V1.Responses.User;
using Bingo.Contracts.V1.Responses.UserReport;
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


            // for create post
            CreateMap<Post, CreatePostResponse>()
                .ForMember(dest => dest.Tags, opt =>
                    opt.MapFrom(src => src.Tags.Select(x => x.Tag.TagName)));

            // used for GetPost
            CreateMap<Post, PostResponse>()
                .ForPath(dest => dest.Location.Id, opt => opt.MapFrom(src => src.Location.Id))
                .ForPath(dest => dest.Location.Longitude, opt => opt.MapFrom(src => src.Location.Location.X))
                .ForPath(dest => dest.Location.Latitude, opt => opt.MapFrom(src => src.Location.Location.Y))
                .ForPath(dest => dest.Location.Region, opt => opt.MapFrom(src => src.Location.Region))
                .ForPath(dest => dest.Location.Address, opt => opt.MapFrom(src => src.Location.Address))
                .ForPath(dest => dest.Location.EntityName, opt => opt.MapFrom(src => src.Location.EntityName))
                .ForPath(dest => dest.Location.City, opt => opt.MapFrom(src => src.Location.City))
                .ForPath(dest => dest.Location.Country, opt => opt.MapFrom(src => src.Location.Country))
                .ForPath(dest => dest.Event.Id, opt => opt.MapFrom(src => src.Event.Id))
                .ForPath(dest => dest.Event.Description, opt => opt.MapFrom(src => src.Event.Description))
                .ForPath(dest => dest.Event.Requirements, opt => opt.MapFrom(src => src.Event.Requirements))
                .ForPath(dest => dest.Event.Title, opt => opt.MapFrom(src => src.Event.Title))
                .ForPath(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags.Select(x => x.Tag.TagName)))
                .ForPath(dest => dest.Event.EntrancePrice, opt => opt.MapFrom(src => src.Event.EntrancePrice ?? 0))
                .ForMember(dest => dest.RepeatablePropertyDataId, src => src.MapFrom(s => s.Id))
                .ForMember(dest => dest.VoucherDataId, src => src.MapFrom(s => s.Id))
                .ForMember(dest => dest.AnnouncementsDataId, src => src.MapFrom(s => s.Id))
                .ForMember(dest => dest.AttendanceDataId, src => src.MapFrom(s => s.Id));
                

            // For update post
            CreateMap<EventLocation, UpdatedLocation>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(s => s.Id))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(s => s.Location.Y))
                .ForMember(dest => dest.Logitude, opt => opt.MapFrom(s => s.Location.X));
            // for update post
            CreateMap<Models.Event, Bingo.Contracts.V1.Responses.Post.UpdatedEvent>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(s => s.Id));
            // for update post
            CreateMap<Post, UpdatePostResponse>()
                .ForMember(dest => dest.Location, opt => opt.MapFrom(s => s.Location))
                .ForMember(dest => dest.Event, opt => opt.MapFrom(s => s.Event));

            // Get All active attended events
            CreateMap<Post, ActiveAttendedEvent>()
                .ForMember(dest => dest.Thumbnail, opt => opt.MapFrom(src => src.Pictures.FirstOrDefault()))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Event.Title))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Event.EntrancePrice))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Location.Address));

            // Get atendees
            CreateMap<AppUser, EventParticipant>()
                .ForMember(dest => dest.Picture, opt => opt.MapFrom(src => src.ProfilePicture));

            // Announcement model to response , viceversa
            CreateMap<Announcement, CreateAnnouncementResponse>();
            CreateMap<Announcement, GetAnnouncement>();

            // Map Ratings
            CreateMap<Rating, CreateRatingResponse>();
            CreateMap<Rating, GetRating>();

            // Map Reports
            CreateMap<Report, CreateReportResponse>();
            CreateMap<Report, ReportResponse>();

            // Map Profile
            CreateMap<AppUser, ProfileResponse>();

            // Map UserReports
            CreateMap<UserReport, CreateUserReportResponse>();
            CreateMap<UserReport, UserReportResponse>();

        }
        
    }
}
