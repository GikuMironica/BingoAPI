using Bingo.Contracts.V1.Requests.Post;
using BingoAPI.Models;
using Microsoft.AspNetCore.Http;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
                EndTime = postRequest.EndTime,
                PostTime = postRequest.PostTime,
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
            post.Pictures = new List<string>();
            post.Tags = new List<PostTags>();
            if (postRequest.Tags != null) { 
                foreach (var tag in postRequest.Tags)
                {
                    if (tag != null)
                    {
                      post.Tags.Add(new PostTags { Tag = new Tag { TagName = tag } });
                    }
                }
            }
            return post;
        }

        public Event DiscriminateEvent(ContainedEvent containedEvent)
        {
            Event generatedEvent;
            switch (containedEvent.EventType)
            {
                case 1:
                    generatedEvent = new HouseParty
                    {
                        Description = containedEvent.Description,
                        Requirements = containedEvent.Requirements,
                        Slots = containedEvent.Slots,
                        EntrancePrice = containedEvent.EntrancePrice
                    };
                    break;
                case 2:
                    generatedEvent = new Club
                    {
                        Description = containedEvent.Description,
                        Requirements = containedEvent.Requirements,
                        EntrancePrice = containedEvent.EntrancePrice ?? 0                        
                    };
                    break;
                case 3:
                    generatedEvent = new Bar
                    {
                        Description = containedEvent.Description,
                        Requirements = containedEvent.Requirements,
                        EntrancePrice = containedEvent.EntrancePrice ?? 0
                    };
                    break;
                case 4:
                    generatedEvent = new BikerMeet
                    {
                        Description = containedEvent.Description,
                        Requirements = containedEvent.Requirements
                    };
                    break;
                case 5:
                    generatedEvent = new BicycleMeet
                    {
                        Description = containedEvent.Description,
                        Requirements = containedEvent.Requirements
                    };
                    break;
                case 6:
                    generatedEvent = new CarMeet
                    {
                        Description = containedEvent.Description,
                        Requirements = containedEvent.Requirements
                    };
                    break;
                case 7:
                    generatedEvent = new StreetParty
                    {
                        Description = containedEvent.Description,
                        Requirements = containedEvent.Requirements
                    };
                    break;
                case 8:
                    generatedEvent = new Marathon
                    {
                        Description = containedEvent.Description,
                        Requirements = containedEvent.Requirements
                    };
                    break;
                case 9:
                    generatedEvent = new FlashMob
                    {
                        Description = containedEvent.Description,
                        Requirements = containedEvent.Requirements
                    };
                    break;
                case 10:
                    generatedEvent = new Other
                    {
                        Description = containedEvent.Description,
                        Requirements = containedEvent.Requirements
                    };
                    break;
                default:
                    generatedEvent = new Other
                    {
                        Description = containedEvent.Description,
                        Requirements = containedEvent.Requirements
                    };
                    break;
                   
            }

            return generatedEvent;
        }
    }
}
