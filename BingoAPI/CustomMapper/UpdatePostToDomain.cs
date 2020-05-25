using AutoMapper;
using Bingo.Contracts.V1.Requests.Post;
using BingoAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.CustomMapper
{
    public class UpdatePostToDomain : IUpdatePostToDomain
    {
        private readonly IMapper mapper;

        public UpdatePostToDomain(IMapper mapper)
        {
            this.mapper = mapper;
        }

        public Post Map(UpdatePostRequest updatePostRequest, Post post)
        {
            // map all properties except the tags
            mapper.Map<UpdatePostRequest, Post>(updatePostRequest, post);

            if(updatePostRequest.EventTime != null)
            {
                post.EventTime = updatePostRequest.EventTime.Value;
            }
            if (updatePostRequest.Event !=null && updatePostRequest.Event.Slots.HasValue)
            {
                if (post.Event.GetType().ToString().Contains("HouseParty"))
                {
                    ((HouseParty)(post.Event)).Slots = updatePostRequest.Event.Slots.Value;
                }                    
            }

            // if updateRequest has tags, delete all tags  from post and assign to it the tags from request object
            if (updatePostRequest.TagNames != null)
            {
                post.Tags = new List<PostTags>();

                foreach (var tag in updatePostRequest.TagNames)
                {
                    if (tag != null && tag.Length > 0)
                    {
                        post.Tags.Add(new PostTags { Tag = new Tag { TagName = tag, Counter = 1 } });
                    }
                }
            }

            return post;
        }
    }
}
