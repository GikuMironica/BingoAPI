using Bingo.Contracts.V1.Requests.Post;
using BingoAPI.Models;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BingoAPI.CustomMapper
{
    public class CreatePostRequestMapper : ICreatePostRequestMapper
    {
        public Post MapRequestToDomain(CreatePostRequest postRequest, AppUser user)
        {
            var containedEvent = DiscriminateEvent(postRequest.Event);
            var post = new Post
            {
                Event = containedEvent,
                EventTime = postRequest.EventTime,
                EndTime = postRequest.EndTime ?? postRequest.EventTime + 25200,
                PostTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                User = user,
                Location = new EventLocation
                {
                    Location = new Point(postRequest.UserLocation.Longitude, postRequest.UserLocation.Latitude),
                    Address = postRequest.UserLocation.Address,
                    City = postRequest.UserLocation.City,
                    Country = postRequest.UserLocation.Country,
                    Region = postRequest.UserLocation.Region,
                    EntityName = postRequest.UserLocation.EntityName
                }
            };
            post.Event.Post = post;
            post.Pictures = new List<Picture>();
            post.Tags = new List<PostTags>();
            if (postRequest.Tags == null) return post;
            foreach (var tag in postRequest.Tags.Where(tag => tag != null))
            {
                post.Tags.Add(new PostTags { Tag = new Tag { TagName = tag } });
            }
            return post;
        }

        public Event DiscriminateEvent(ContainedEvent containedEvent)
        {
            Event generatedEvent = containedEvent.EventType switch
            {
                1 => new HouseParty
                {
                    Description = containedEvent.Description,
                    Requirements = containedEvent.Requirements,
                    Slots = containedEvent.Slots,
                    EntrancePrice = containedEvent.EntrancePrice,
                    Currency = containedEvent.Currency.GetValueOrDefault(0),
                    Title = containedEvent.Title
                },
                2 => new Club
                {
                    Description = containedEvent.Description,
                    Requirements = containedEvent.Requirements,
                    EntrancePrice = containedEvent.EntrancePrice ?? 0,
                    Currency = containedEvent.Currency.GetValueOrDefault(0),
                    Title = containedEvent.Title
                },
                3 => new Bar
                {
                    Description = containedEvent.Description,
                    Requirements = containedEvent.Requirements,
                    EntrancePrice = containedEvent.EntrancePrice ?? 0,
                    Currency = containedEvent.Currency.GetValueOrDefault(0),
                    Title = containedEvent.Title
                },
                4 => new BikerMeet
                {
                    Description = containedEvent.Description,
                    Requirements = containedEvent.Requirements,
                    Title = containedEvent.Title
                },
                5 => new BicycleMeet
                {
                    Description = containedEvent.Description,
                    Requirements = containedEvent.Requirements,
                    Title = containedEvent.Title
                },
                6 => new CarMeet
                {
                    Description = containedEvent.Description,
                    Requirements = containedEvent.Requirements,
                    Title = containedEvent.Title
                },
                7 => new StreetParty
                {
                    Description = containedEvent.Description,
                    Requirements = containedEvent.Requirements,
                    Title = containedEvent.Title
                },
                8 => new Marathon
                {
                    Description = containedEvent.Description,
                    Requirements = containedEvent.Requirements,
                    Title = containedEvent.Title
                },
                9 => new Other
                {
                    Description = containedEvent.Description,
                    Requirements = containedEvent.Requirements,
                    Title = containedEvent.Title
                },
                _ => new Other
                {
                    Description = containedEvent.Description,
                    Requirements = containedEvent.Requirements,
                    Title = containedEvent.Title
                }
            };

            return generatedEvent;
        }
    }
}
