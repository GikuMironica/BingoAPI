using AutoMapper;
using Bingo.Contracts.V1.Requests.Post;
using BingoAPI.Models;
using System.Collections.Generic;
using System.Linq;

namespace BingoAPI.CustomMapper
{
    public class UpdatePostToDomain : IUpdatePostToDomain
    {
        private readonly IMapper _mapper;

        public UpdatePostToDomain(IMapper mapper)
        {
            this._mapper = mapper;
        }

        public Post Map(UpdatePostRequest updatePostRequest, Post post)
        {
            // map all properties except the tags
            _mapper.Map<UpdatePostRequest, Post>(updatePostRequest, post);

            if (updatePostRequest.EventTime != null)
            {
                post.EventTime = updatePostRequest.EventTime.Value;
            }
            if (!post.Event.GetType().ToString().Contains("HouseParty") 
                && !post.Event.GetType().ToString().Contains("Bar") 
                && !post.Event.GetType().ToString().Contains("Club"))
            {
                post.Event.EntrancePrice = 0;
                post.Event.Currency = null;
            }
            if (updatePostRequest.UpdatedEvent?.Slots != null)
            {
                if (post.Event.GetType().ToString().Contains("HouseParty"))
                {
                    ((HouseParty)(post.Event)).Slots = updatePostRequest.UpdatedEvent.Slots.Value;
                }                    
            }

            // Temp workaround for Requirements - (can't be deleted)
            if(updatePostRequest.UpdatedEvent != null && 
                updatePostRequest.UpdatedEvent.Requirements != null && 
                updatePostRequest.UpdatedEvent.Requirements == ":~+_77?!")
            {
                post.Event.Requirements = null;
            }

            // if updateRequest has tags, delete all tags  from post and assign to it the tags from request object
            if (updatePostRequest.TagNames == null) return post;
            post.Tags = new List<PostTags>();

            foreach (var tag in updatePostRequest.TagNames.Where(tag => !string.IsNullOrEmpty(tag)))
            {
                post.Tags.Add(new PostTags { Tag = new Tag { TagName = tag } });
            }

            return post;
        }
    }
}
