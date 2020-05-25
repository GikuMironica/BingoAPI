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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
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
        private readonly IImageToWebpProcessor imageToWebpProcessor;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly IAwsBucketManager awsBucketManager;
        private readonly ILogger<PostController> logger;
        private readonly IUriService uriService;
        private readonly IUpdatePostToDomain updatePostToDomain;

        public PostController(IOptions<EventTypes> eventTypes, IMapper mapper, ICreatePostRequestMapper createPostRequestMapper
                              ,UserManager<AppUser> userManager, IPostsRepository postRepository, IImageToWebpProcessor imageToWebpProcessor
                              ,IWebHostEnvironment webHostEnvironment, IAwsBucketManager awsBucketManager, ILogger<PostController> logger
                              ,IUriService uriService, IUpdatePostToDomain updatePostToDomain)
        {
            this.eventTypes = eventTypes.Value;
            this.mapper = mapper;
            this.createPostRequestMapper = createPostRequestMapper;
            this.userManager = userManager;
            this.postRepository = postRepository;
            this.imageToWebpProcessor = imageToWebpProcessor;
            this.webHostEnvironment = webHostEnvironment;
            this.awsBucketManager = awsBucketManager;
            this.logger = logger;
            this.uriService = uriService;
            this.updatePostToDomain = updatePostToDomain;
        }

        /// <summary>
        /// This endpoint returns relevant data about a post
        /// it includes the owner id, the location, pictures, tags
        /// </summary>
        /// <param name="postId">The post Id</param>
        /// <response code="200">The post was found and returned</response>
        [HttpGet(ApiRoutes.Posts.Get)]
        [ProducesResponseType(typeof(Response<PostResponse>), 200)]
        public async Task<IActionResult> Get([FromRoute] int postId)
        {
            var post = await postRepository.GetByIdAsync(postId);
            if (post == null)
                return NotFound();

            var response = new Response<PostResponse>(mapper.Map<PostResponse>(post));

            string eventType = post.Event.GetType().Name.ToString();
            
            var eventTypeNumber = eventTypes.Types
                .Where(y => y.Type == eventType)
                .Select(x => x.Id)
                .FirstOrDefault();

            response.Data.Event.EventType = eventTypeNumber;
            // return slots if house party
            if (eventTypeNumber == 1)
            {
                response.Data.Event.Slots = ((HouseParty)(post.Event)).Slots;
            }
            
            return Ok(response);
        }



        /// <summary>
        /// This endpoint returns all active events base on users location.
        /// By default it returns the events within 30km range
        /// </summary>
        /// <param name="getAllRequest"></param>
        /// <returns></returns>
        [HttpGet(ApiRoutes.Posts.GetAll)]
        public async Task<IActionResult> GetAll(GetAllRequest getAllRequest)
        {
            Point userLocation = new Point(getAllRequest.UserLocation.Longitude, getAllRequest.UserLocation.Latitude);
            var posts = postRepository.GetAllAsync(userLocation, getAllRequest.UserLocation.RadiusRange ?? 20);

            if (posts == null || posts.Count() == 0)
            {
                return Ok(new Response<string> { Data = "No events in your area" });
            }

            var resultList = new List<Posts>();
            
            foreach(var post in posts)
            {
                string eventType = post.Event.GetType().Name.ToString();

                var eventTypeNumber = eventTypes.Types
                .Where(y => y.Type == eventType)
                .Select(x => x.Id)
                .FirstOrDefault();

                var dist = post.Location.Location.Distance(userLocation);

                resultList.Add(new Posts 
                { 
                    PostId = post.Id, Address = post.Location.Address, 
                    Description = post.Event.Description, Thumbnail = post.Pictures.FirstOrDefault(),
                    PostType = eventTypeNumber
                    
                });
            }

            return Ok(new Response<List<Posts>>{ Data = resultList });
        }



        /// <summary>
        /// This endpoint is used for creating posts
        /// A post includes location, event, pictures, tags.
        /// </summary>
        /// <param name="postRequest">request object</param>
        /// <response code="201">Post successfuly created</response>
        /// <response code="400">Post could not be persisted, due to missing required data or corrupt images</response>
        [HttpPost(ApiRoutes.Posts.Create)]
        [ProducesResponseType(typeof(Response<CreatePostResponse>), 201)]
        public async Task<IActionResult> Create( CreatePostRequest postRequest)
        {
            
            var User = await userManager.FindByIdAsync(HttpContext.GetUserId());

            // Map request to domain
            var post = createPostRequestMapper.MapRequestToDomain(postRequest, User);
            post.ActiveFlag = 1;

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
                    var uploadResult = await awsBucketManager.UploadFileAsync(imageProcessingResult);
                    if (!uploadResult.Result)
                    {
                        return BadRequest(new SingleError { Message = "The provided images couldn't be stored. Try to upload other pictures." });
                    }
                    post.Pictures.AddAllIfNotNull(uploadResult.ImageNames);

                }
                else { return BadRequest(new SingleError { Message = imageProcessingResult.ErrorMessage }); }
            }            

            var result = await postRepository.AddAsync(post);

            if (!result)
                return BadRequest();

            var locationUri = uriService.GetPostUri(post.Id.ToString());

            return Created(locationUri, new Response<CreatePostResponse>(mapper.Map<CreatePostResponse>(post)));
        }



        /// <summary>
        /// This endpoint is used to update a post with it's related data which are all optional
        /// Event Location, The contained Event, The Post itself, Tags and Pictures can be updated here
        /// </summary>
        /// <param name="postId">The post id</param>
        /// <param name="postRequest">The request object, all attributes are optional</param>
        /// <response code="200"></response>
        [HttpPut(ApiRoutes.Posts.Update)]
        public async Task<IActionResult> Update([FromRoute] int postId, UpdatePostRequest postRequest)
        {
            var userisOwnerOrAdmin = await postRepository.IsPostOwnerOrAdminAsync(postId, HttpContext.GetUserId());

            if (!userisOwnerOrAdmin)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "You do not own this post / You are not an Administrator" });
            }

            var post = await postRepository.GetPostByIdAsync(postId);

            if (post == null)
                return NotFound();

            // map request object to existing post
            Post mappedPost = updatePostToDomain.Map(postRequest, post);

            // Get the Guid of the deleted pictures
            if (postRequest.RemainingImagesGuids == null)
                postRequest.RemainingImagesGuids = new List<string>();
            List<string> deletedImages = post.Pictures.Except(postRequest.RemainingImagesGuids).ToList();

            // delete from the S3 bucket the delete pictures
            if (deletedImages.Count > 0)
            {
                var deletedPicturesResult = await awsBucketManager.DeleteFileAsync(deletedImages);
                if (!deletedPicturesResult.Result)
                {
                    // log the Delete Exceptions list
                }

                // remove the deleted Guids from post                            
                mappedPost.Pictures = postRequest.RemainingImagesGuids;
            }                

            // process the new images if there are any
            ImageProcessingResult imageProcessingResult = null;
            List<IFormFile> pictures = new List<IFormFile>();
            pictures.AddAllIfNotNull(
                new List<IFormFile> { postRequest.Picture1, postRequest.Picture2, postRequest.Picture4, postRequest.Picture4, postRequest.Picture5 });

            if ((pictures.Count > 0))
            {
                mappedPost.Pictures = new List<string>();
                imageProcessingResult = imageToWebpProcessor.ConvertFiles(pictures);

                // add images
                if (imageProcessingResult.Result)
                {
                    // upload images to cdn, assign image links to post.Pics
                    var uploadResult = await awsBucketManager.UploadFileAsync(imageProcessingResult);
                    if (!uploadResult.Result)
                    {
                        return BadRequest(new SingleError { Message = "The provided images couldn't be stored. Try to upload other pictures." });
                    }
                    mappedPost.Pictures.AddAllIfNotNull(uploadResult.ImageNames);

                }
                else { return BadRequest(new SingleError { Message = imageProcessingResult.ErrorMessage }); }
            }


            // update in the system the modified post
            var updated = await postRepository.UpdateAsync(mappedPost);

            // map to a response object if updated
            if (updated)
            {
                var locationUri = uriService.GetPostUri(post.Id.ToString());
                return Ok(locationUri/*new Response<UpdatePostResponse>(mapper.Map<UpdatePostResponse>(mappedPost))*/);
            }

            return BadRequest();
        }


        /// <summary>
        /// This endpoint is used for deleting posts.
        /// A post can only be deleted by it's owner or by Admin / SuperAdmin
        /// It will delete all related data, like Event, EventLocation but will leave the created tags.
        /// </summary>
        /// <param name="postId">The post id</param>
        /// <response code="204">Post was successfuly deleted</response>
        /// <response code="403">You do not own this post / You are not an Administrator</response>
        /// <response code="404">Post was not found</response>
        [HttpDelete(ApiRoutes.Posts.Delete)]
        [ProducesResponseType(typeof(SingleError), 403)]
        public async Task<IActionResult> Delete([FromRoute] int postId )
        {
            var userOwnsPost = await postRepository.IsPostOwnerOrAdminAsync(postId, HttpContext.GetUserId());

            if (!userOwnsPost)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "You do not own this post / You are not an Administrator" });
            }


            var post = await postRepository.GetPostByIdAsync(postId);
            List<string> deletedImagesList = post.Pictures;

            // delete from the S3 bucket the delete pictures
            if (deletedImagesList.Count > 0)
            {
                var deletedPicturesResult = await awsBucketManager.DeleteFileAsync(deletedImagesList);
                if (!deletedPicturesResult.Result)
                {
                    // log the Delete Exceptions list
                }
            }

            var deleted = await postRepository.DeleteAsync(postId);

            if (deleted)
                return NoContent();

            return NotFound();
        }

    }
}
