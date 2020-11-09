using AutoMapper;
using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.Post;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.Post;
using BingoAPI.Cache;
using BingoAPI.CustomMapper;
using BingoAPI.CustomValidation;
using BingoAPI.Domain;
using BingoAPI.Extensions;
using BingoAPI.Helpers;
using BingoAPI.Models;
using BingoAPI.Models.SqlRepository;
using BingoAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Admin,User")]
    [Produces("application/json")]
    public class PostController : Controller
    {
        private readonly EventTypes _eventTypes;
        private readonly IMapper _mapper;
        private readonly ICreatePostRequestMapper _createPostRequestMapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly IPostsRepository _postRepository;
        private readonly IAwsBucketManager _awsBucketManager;
        private readonly IUriService _uriService;
        private readonly IUpdatePostToDomain _updatePostToDomain;
        private readonly IImageLoader _imageLoader;
        private readonly IDomainToResponseMapper _domainToResponseMapper;
        private readonly INotificationService _notificationService;
        private readonly IUpdatedPostDetailsWatcher _postDetailsWatcher;
        private readonly IRatingRepository _ratingRepository;
        private readonly IEventAttendanceRepository _attendanceRepository;
        private readonly IRequestToDomainMapper _requestToDomainMapper;

        public PostController(IOptions<EventTypes> eventTypes, IMapper mapper, ICreatePostRequestMapper createPostRequestMapper
                              , UserManager<AppUser> userManager, IPostsRepository postRepository, IAwsBucketManager awsBucketManager, IUriService uriService, IUpdatePostToDomain updatePostToDomain, IImageLoader imageLoader, IDomainToResponseMapper domainToResponseMapper
                              , INotificationService notificationService, IUpdatedPostDetailsWatcher postDetailsWatcher
                              , IRatingRepository ratingRepository, IEventAttendanceRepository attendanceRepository, IRequestToDomainMapper requestToDomainMapper)
        {
            this._eventTypes = eventTypes.Value;
            this._mapper = mapper;
            this._createPostRequestMapper = createPostRequestMapper;
            this._userManager = userManager;
            this._postRepository = postRepository;
            this._awsBucketManager = awsBucketManager;
            this._uriService = uriService;
            this._updatePostToDomain = updatePostToDomain;
            this._imageLoader = imageLoader;
            this._domainToResponseMapper = domainToResponseMapper;
            this._notificationService = notificationService;
            this._postDetailsWatcher = postDetailsWatcher;
            this._ratingRepository = ratingRepository;
            this._attendanceRepository = attendanceRepository;
            this._requestToDomainMapper = requestToDomainMapper;
        }

        /// <summary>
        /// This endpoint returns relevant data about a post.
        /// It includes the host id, the location, pictures, tags and other details.
        /// </summary>
        /// <param name="postId">The post Id</param>
        /// <response code="200">The post was found and returned</response>
        /// <response code="404">The post was not found</response>
        //[Cached(300)]
        [HttpGet(ApiRoutes.Posts.Get)]
        [ProducesResponseType(typeof(Response<PostResponse>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Get([FromRoute] int postId)
        {            
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null)
                return NotFound();

            var response = new Response<PostResponse>(_mapper.Map<PostResponse>(post));

            string eventType = post.Event.GetType().Name;

            var eventTypeNumber = _eventTypes.Types
                .Where(y => y.Type == eventType)
                .Select(x => x.Id)
                .FirstOrDefault();

            if(eventTypeNumber == 1)
            {
                response.Data.AvailableSlots = await _postRepository.GetAvailableSlotsAsync(postId);
            }

            response.Data.IsAttending = await _attendanceRepository.IsUserAttendingEvent(HttpContext.GetUserId(), postId);

            response.Data.Event.EventType = eventTypeNumber;
            response.Data.HostRating = await _ratingRepository.GetUserRating(post.UserId);
            response.Data.Event.Slots = post.Event.GetSlotsIfAny();
            return Ok(response);
        }


        /// <summary>
        /// This endpoint is used to fetch all currently active events hosted by the requester.
        /// The user data is retrieved from the JWT.
        /// </summary>
        /// <param name="paginationQuery">Contains the pagination details, like page number and page size. Default values are 1 for the page number and 50 for page size</param>
        /// <response code="200">Returns the standard mini post</response>
        /// <response code="204">User has no active posts</response>
        [ProducesResponseType(typeof(PagedResponse<Posts>), 200)]
        [ProducesResponseType(204)]
        [HttpGet(ApiRoutes.Posts.GetAllActive)]
        public async Task<IActionResult> GetMyActiveEvents([FromQuery] PostsPaginationQuery paginationQuery)
        {
            var userId = HttpContext.GetUserId();
            var paginationFilter = _mapper.Map<PaginationFilter>(paginationQuery);
            var posts = await _postRepository.GetMyActive(userId, paginationFilter);

            if (!posts.Any())
            {
                return NoContent();
            }

            var resultList = new List<Posts>();
            foreach (var post in posts)
            {
                var mappedPost = _domainToResponseMapper.MapPostForGetAllPostsReponse(post, _eventTypes);
                mappedPost.Slots = post.Event.GetSlotsIfAny();
                mappedPost.HostRating = await _ratingRepository.GetUserRating(post.UserId);
                resultList.Add(mappedPost);
            }
            var paginationResponse = PaginationHelpers.CreatePaginatedResponse(_uriService, paginationFilter, resultList);
            return Ok(paginationResponse);
        }


        /// <summary>
        /// This end point is used to fetch all inactive events hosted by the requester.
        /// The user data is retrieved from the JWT.
        /// </summary>
        /// <param name="paginationQuery">Containes the pagination details, like page number and page size. Default values are 1 for the page number and 50 for page size</param>
        /// <response code="200">Returns the standard mini post</response>
        /// <response code="204">User did not host any event yet</response>
        [ProducesResponseType(typeof(PagedResponse<Posts>), 200)]
        [ProducesResponseType(204)]
        [HttpGet(ApiRoutes.Posts.GetAllInactive)]
        public async Task<IActionResult> GetMyInactiveEvents([FromQuery] PostsPaginationQuery paginationQuery)
        {
            var userId = HttpContext.GetUserId();
            var paginationFilter = _mapper.Map<PaginationFilter>(paginationQuery);
            var posts = await _postRepository.GetMyInactive(userId, paginationFilter);

            if (!posts.Any())
            {
                return NoContent();
            }

            var resultList = new List<Posts>();
            foreach (var post in posts)
            {
                var mappedPost = _domainToResponseMapper.MapPostForGetAllPostsReponse(post, _eventTypes);
                mappedPost.Slots = post.Event.GetSlotsIfAny();
                mappedPost.HostRating = await _ratingRepository.GetUserRating(post.UserId);
                resultList.Add(mappedPost);
            }
            var paginationResponse = PaginationHelpers.CreatePaginatedResponse(_uriService, paginationFilter, resultList);
            return Ok(paginationResponse);
        }



        /// <summary>
        /// This endpoint returns all active events based on the requester's location.
        /// By default it returns the events within 15km range
        /// </summary>
        /// <param name="getAllRequest">Contains users Longitude,Latitude and search range</param>
        /// <param name="filteredGetAll">Event types to be included in the search result. If all are null or false, all of them will be included in the result. 
        /// It also contains an option to returns the events which will occur today. And lastly, Tag, will return posts containing this tag.</param>
        /// <response code="200">Returns the standard mini post</response>
        /// <response code="204">No active events in this area</response>
        [ProducesResponseType(typeof(Response<List<Posts>>), 200)]
        [ProducesResponseType(204)]
        [Cached(300)]
        [HttpGet(ApiRoutes.Posts.GetAll)]
        public async Task<IActionResult> GetAll(GetAllRequest getAllRequest, FilteredGetAllPostsRequest filteredGetAll)
        {
            Point userLocation = new Point(getAllRequest.UserLocation.Longitude, getAllRequest.UserLocation.Latitude);
            var filter = _requestToDomainMapper.MapPostFilterRequestToDomain(_mapper, filteredGetAll);
            Int64 today = 15778476 + DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (filteredGetAll.Today.GetValueOrDefault(false))
            {
                today = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 57600;
            }

            var posts = await _postRepository.GetAllAsync(userLocation, getAllRequest.UserLocation.RadiusRange, filter, today, filteredGetAll.Tag ?? "%");
            if (posts == null || !posts.Any())
            {
                return NoContent();
            }

            var resultList = new List<Posts>();

            foreach (var post in posts)
            {
                var mappedPost = _domainToResponseMapper.MapPostForGetAllPostsReponse(post, _eventTypes);
                mappedPost.Slots = post.Event.GetSlotsIfAny();
                mappedPost.HostRating = await _ratingRepository.GetUserRating(post.UserId);
                resultList.Add(mappedPost);
            }

            return Ok(new Response<List<Posts>> { Data = resultList });
        }


        

        /// <summary>
        /// This endpoint is used for creating posts.
        /// A post includes location, event, pictures, tags and other details.
        /// </summary>
        /// <param name="postRequest">request object</param>
        /// <response code="201">Post successfully created</response>
        /// <response code="400">Post could not be persisted, due to missing required data or corrupt images</response>
        [HttpPost(ApiRoutes.Posts.Create)]
        [Authorize(Policy="CreateEditPost")]
        [ProducesResponseType(typeof(Response<CreatePostResponse>), 201)]
        [ProducesResponseType(typeof(SingleError), 400)]
        public async Task<IActionResult> Create([FromForm]CreatePostRequest postRequest)
        {
            var user = await _userManager.FindByIdAsync(HttpContext.GetUserId());
            if(user.FirstName == null || user.LastName == null)
            {
                return BadRequest(new SingleError { Message = "User has to input first and last name in order to create post" });
            }
            var activePosts = await _postRepository.GetActiveEventsNumbers(HttpContext.GetUserId());
            if(activePosts != 0)
            {
                var isAdmin = await RoleCheckingHelper.CheckIfAdmin(_userManager, user);
                if(!isAdmin)
                    return BadRequest(new SingleError { Message = "Basic user can't have more than 1 active event at a time" });
            }

            var post = _createPostRequestMapper.MapRequestToDomain(postRequest, user);
            post.ActiveFlag = 1;

            var imagesProcessingResult = await ProcessImagesAsync(postRequest.Picture1, postRequest.Picture2, postRequest.Picture3, post);
            if (!imagesProcessingResult.Result) { return BadRequest(new SingleError { Message = imagesProcessingResult.ErrorMessage }); }

            var result = await _postRepository.AddAsync(post);
            if (!result)
                return BadRequest();

            var mappedPost = _domainToResponseMapper.MapPostForGetAllPostsReponse(post, _eventTypes);
            mappedPost.Slots = post.Event.GetSlotsIfAny();
            mappedPost.HostRating = await _ratingRepository.GetUserRating(post.UserId);

            var locationUri = _uriService.GetPostUri(post.Id.ToString());
            return Created(locationUri, new Response<Posts>(mappedPost));
        }




        /// <summary>
        /// This endpoint is used to update a post with it's related data which are all optional.
        /// Event Location, The contained Event, The Post itself, Tags and Pictures can be updated here by the even host or admin only.
        /// </summary>
        /// <param name="postId">The post id</param>
        /// <param name="postRequest">The request object, all attributes are optional</param>
        /// <response code="200">Update successful</response>
        /// <response code="400">Attempt to input invalid data</response>
        /// <response code="403">Not authorized for this action</response>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(SingleError), 400)]
        [ProducesResponseType(typeof(SingleError), 403)]
        [HttpPost(ApiRoutes.Posts.Update)]
        public async Task<IActionResult> Update(int postId, [FromForm]UpdatePostRequest postRequest)
        {
            var userisOwnerOrAdmin = await _postRepository.IsPostOwnerOrAdminAsync(postId, HttpContext.GetUserId());
            if (!userisOwnerOrAdmin)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "You do not own this post / You are not an Administrator" });
            }

            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null)
                return NotFound();

            // make sure time is updated correctly
            var validatedTime = _postDetailsWatcher.ValidateUpdatedTime(postRequest, post);
            if (!validatedTime.Result)
            {
                return BadRequest(new SingleError { Message = validatedTime.ErrorMessage });
            }

            Post mappedPost = _updatePostToDomain.Map(postRequest, post);

            // delete existing pictures -> rep\upload
            await DeletePicturesAsync(postRequest, mappedPost);
            var imagesProcessingResult = await ProcessImagesAsync(postRequest.Picture1, postRequest.Picture2, postRequest.Picture3, mappedPost);
            if (!imagesProcessingResult.Result)
            {
                return BadRequest(new SingleError { Message = imagesProcessingResult.ErrorMessage });
            }

            var updated = await _postRepository.UpdateAsync(mappedPost);
            if (updated)
            {
                // notify attendee if something important got updated
                if (_postDetailsWatcher.GetValidatedFields(postRequest))
                {
                    var participants = await _postRepository.GetParticipantsIdAsync(post.Id);
                    if (participants.Count != 0)
                    {
                        await _notificationService.NotifyParticipantsEventUpdatedAsync(participants, mappedPost.Event.Title);
                    }
                }                

                //var locationUri = uriService.GetPostUri(post.Id.ToString());
                return Ok(/*locationUri new Response<UpdatePostResponse>(mapper.Map<UpdatePostResponse>(mappedPost))*/);
            }
            return BadRequest();
        }


        /// <summary>
        /// This endpoint is used for deleting posts.
        /// A post can only be deleted by it's owner or by Admin
        /// It will delete all related data, like Event, Location but, it will leave the created tags.
        /// </summary>
        /// <param name="postId">The post id</param>
        /// <response code="204">Post was successfully deleted</response>
        /// <response code="403">You do not own this post / You are not an Administrator</response>
        /// <response code="404">Post was not found</response>
        [HttpDelete(ApiRoutes.Posts.Delete)]
        [ProducesResponseType(typeof(SingleError), 403)]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Delete([FromRoute] int postId )
        {
            var userOwnsPost = await _postRepository.IsPostOwnerOrAdminAsync(postId, HttpContext.GetUserId());

            if (!userOwnsPost)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "You do not own this post / You are not an Administrator" });
            }

            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null)
            {
                 return NotFound();
            }
            List<string> deletedImagesList = post.Pictures;

            // delete from the S3 bucket the delete pictures
            if (deletedImagesList != null && deletedImagesList.Count > 0)
            {
                // errors logged in bucketManager
                var deletedPicturesResult = await _awsBucketManager.DeleteFileAsync(deletedImagesList, AwsAssetsPath.PostPictures);                
            }

            // notify users
            var participants = await _postRepository.GetParticipantsIdAsync(post.Id);
            if (participants.Count !=0)
            {
                await _notificationService.NotifyParticipantsEventDeletedAsync(participants, post.Event.Title);
            }

            var deleted = await _postRepository.DeleteAsync(postId);
            if (deleted)
                return NoContent();

            return BadRequest();
        }


        /// <summary>
        /// This endpoint is used by admins to disable posts
        /// </summary>
        /// <param name="disableRequest">Contains the post id</param>
        /// <response code="200">Success</response>
        /// <response code="404">Post not found</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Admin")]
        [HttpPut(ApiRoutes.Posts.DisablePost)]
        public async Task<IActionResult> Disable([FromBody] DisablePostRequest disableRequest)
        {
            var post = await _postRepository.GetPlainPostAsync(disableRequest.Id);
            if(post == null)
            {
                return NotFound();
            }

            var result = await _postRepository.DisablePost(post);
            if (result)
            {
                return Ok();
            }
            return BadRequest();
        }



        private async Task<ImageProcessingResult> ProcessImagesAsync(IFormFile picture1, IFormFile picture2, IFormFile picture3, Post post)
        {
            List<IFormFile> pictures = new List<IFormFile>();
            pictures.AddAllIfNotNull(
                new List<IFormFile> { picture1, picture2, picture3
                });

            if (pictures.Count + post.Pictures.Count > 3)
                return new ImageProcessingResult { Result = false, ErrorMessage = "Can't upload more than 3 pictures" };

            if ((pictures.Count > 0))
            {
                ImageProcessingResult imageProcessingResult = await _imageLoader.LoadFiles(pictures);

                if (imageProcessingResult.Result)
                {
                    imageProcessingResult.BucketPath = AwsAssetsPath.PostPictures;
                    var uploadResult = await _awsBucketManager.UploadFileAsync(imageProcessingResult);
                    if (!uploadResult.Result)
                    {
                        return new ImageProcessingResult{ Result = false, ErrorMessage = "The provided images couldn't be stored. Try to upload other pictures." };
                    }
                    post.Pictures.AddAllIfNotNull(uploadResult.ImageNames);
                }
                else { return new ImageProcessingResult{Result=false, ErrorMessage = imageProcessingResult.ErrorMessage }; }
            }
            return new ImageProcessingResult { Result = true };
        }

        private async Task DeletePicturesAsync(UpdatePostRequest postRequest, Post post)
        {
            if (postRequest.RemainingImagesGuids == null)
                postRequest.RemainingImagesGuids = new List<string>();
            List<string> deletedImages = post.Pictures.Except(postRequest.RemainingImagesGuids).ToList();

            if (deletedImages.Count > 0)
            {
                // errors logged in bucketManager
                var deletedPicturesResult = await _awsBucketManager.DeleteFileAsync(deletedImages, AwsAssetsPath.ProfilePictures);                
                post.Pictures = postRequest.RemainingImagesGuids;
            }
        }

                   
    }
}
