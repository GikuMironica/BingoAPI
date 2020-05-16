using AutoMapper;
using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.Post;
using BingoAPI.CustomMapper;
using BingoAPI.Domain;
using BingoAPI.Extensions;
using BingoAPI.Models;
using BingoAPI.Models.SqlRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin, Admin ,User")]
    [Produces("application/json")]
    public class PostController : Controller
    {
        private readonly EventTypes eventTypes;
        private readonly IMapper mapper;
        private readonly ICreatePostRequestMapper createPostRequestMapper;
        private readonly UserManager<AppUser> userManager;
        private readonly IPostsRepository postRepository;

        public PostController(IOptions<EventTypes> eventTypes, IMapper mapper, ICreatePostRequestMapper createPostRequestMapper
                              ,UserManager<AppUser> userManager, IPostsRepository postRepository)
        {
            this.eventTypes = eventTypes.Value;
            this.mapper = mapper;
            this.createPostRequestMapper = createPostRequestMapper;
            this.userManager = userManager;
            this.postRepository = postRepository;
        }

        [HttpGet(ApiRoutes.Posts.Get)]
        public async Task<IActionResult> Get([FromRoute] int postId)
        {
            return Ok();
        }


        [HttpGet(ApiRoutes.Posts.GetAll)]
        public async Task<IActionResult> GetAll([FromBody] GetAllRequest getAllRequest)
        {
            return Ok();
        }


        [HttpPost(ApiRoutes.Posts.Create)]
        public async Task<IActionResult> Create([FromBody] CreatePostRequest postRequest)
        {
            var User = await userManager.FindByIdAsync(HttpContext.GetUserId());
            var post = createPostRequestMapper.MapRequestToDomain(postRequest, User);
            var result = await postRepository.Add(post);
            if (result)
                return Ok(post);
            else
                return BadRequest();

        } 


        [HttpPut(ApiRoutes.Posts.Update)]
        public async Task<IActionResult> Update([FromRoute] int postId, [FromBody] UpdatePostRequest postRequest)
        {
            return Ok();
        }


        [HttpDelete(ApiRoutes.Posts.Delete)]
        public async Task<IActionResult> Delete([FromRoute] int postId )
        {
            return Ok();
        }

    }
}
