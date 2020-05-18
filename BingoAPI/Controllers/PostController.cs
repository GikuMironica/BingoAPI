using AutoMapper;
using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.Post;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.Post;
using BingoAPI.CustomMapper;
using BingoAPI.Domain;
using BingoAPI.Extensions;
using BingoAPI.Models;
using BingoAPI.Models.SqlRepository;
using BingoAPI.Services;
using ImageProcessor.Plugins.WebP.Imaging.Formats;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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
        private readonly IImageToWebpProcessor imageToWebpProcessor;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IAwsImageUploader awsImageUploader;
        private readonly ILogger<PostController> logger;
        private readonly IUriService uriService;

        public PostController(IOptions<EventTypes> eventTypes, IMapper mapper, ICreatePostRequestMapper createPostRequestMapper
                              ,UserManager<AppUser> userManager, IPostsRepository postRepository, IImageToWebpProcessor imageToWebpProcessor
                              ,IWebHostEnvironment webHostEnvironment, IAwsImageUploader awsImageUploader, ILogger<PostController> logger
                              ,IUriService uriService)
        {
            this.eventTypes = eventTypes.Value;
            this.mapper = mapper;
            this.createPostRequestMapper = createPostRequestMapper;
            this.userManager = userManager;
            this.postRepository = postRepository;
            this.imageToWebpProcessor = imageToWebpProcessor;
            this.webHostEnvironment = webHostEnvironment;
            this.awsImageUploader = awsImageUploader;
            this.logger = logger;
            this.uriService = uriService;
        }

        [HttpGet(ApiRoutes.Posts.Get)]
        public async Task<IActionResult> Get([FromRoute] int postId)
        {
            return Ok();
        }


        [HttpGet(ApiRoutes.Posts.GetAll)]
        public async Task<IActionResult> GetAll([FromRoute] GetAllRequest getAllRequest)
        {
            return Ok();
        }


        [HttpPost(ApiRoutes.Posts.Create)]
        public async Task<IActionResult> Create( CreatePostRequest postRequest)
        {
            
            var User = await userManager.FindByIdAsync(HttpContext.GetUserId());

            // Map request to domain
            var post = createPostRequestMapper.MapRequestToDomain(postRequest, User);


            // Temporary solution - get all non null images from request obj, save in list
            ImageProcessingResult imageProcessingResult = null;
            List<IFormFile> pictures = new List<IFormFile>();
            pictures.AddAllIfNotNull(
                new List<IFormFile> { postRequest.Picture1, postRequest.Picture2, postRequest.Picture4, postRequest.Picture4, postRequest.Picture5 });

            if ((pictures.Count>0))
            {
                imageProcessingResult = imageToWebpProcessor.ConvertFiles(pictures);

                // add images
                if (imageProcessingResult.Result)
                {
                    // upload images to cdn, assign image links to post.Pics
                    var uploadResult = await awsImageUploader.UploadFileAsync(imageProcessingResult);
                    if (!uploadResult.Result)
                    {
                        return BadRequest(new SingleError { Message = "The provided images couldn't be stored. Try to upload other pictures." });
                    }
                    ((List<string>?)(post.Pictures)).AddAllIfNotNull(uploadResult.ImageNames);

                }
                else { return BadRequest(new SingleError { Message = imageProcessingResult.ErrorMessage }); }
            }            

            var result = await postRepository.Add(post);

            if (!result)
                return BadRequest();

            var locationUri = uriService.GetPostUri(post.Id.ToString());

            return Created(locationUri, new Response<CreatePostResponse>(mapper.Map<CreatePostResponse>(post)));
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
