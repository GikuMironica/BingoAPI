using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Responses;
using BingoAPI.Cache;
using BingoAPI.CustomMapper;
using BingoAPI.Domain;
using BingoAPI.Extensions;
using BingoAPI.Models;
using BingoAPI.Models.SqlRepository;
using BingoAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using BingoAPI.Options;

namespace BingoAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Admin,User")]
    [Produces("application/json")]
    public class AttendedEventsController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEventAttendanceRepository _eventAttendanceService;
        private readonly INotificationService _notificationService;
        private readonly IPostsRepository _postsRepository;
        private readonly IDomainToResponseMapper _domainToResponseMapper;
        private readonly IRatingRepository _ratingRepository;
        private readonly EventTypes _eventTypes;

        public AttendedEventsController(UserManager<AppUser> userManager, IEventAttendanceRepository eventAttendanceService, INotificationService notificationService, IPostsRepository postsRepository,
                                        IDomainToResponseMapper domainToResponseMapper, IOptions<EventTypes> eventTypes, IRatingRepository ratingRepository)
        {
            this._userManager = userManager;
            this._eventAttendanceService = eventAttendanceService;
            this._notificationService = notificationService;
            this._postsRepository = postsRepository;
            this._domainToResponseMapper = domainToResponseMapper;
            this._ratingRepository = ratingRepository;
            this._eventTypes = eventTypes.Value;
        }


        /// <summary>
        /// This end point adds the requester to the event participators list.
        /// If the event is a house party, it is added to the pending requests list.
        /// The host will get a push notification about a new request.
        /// </summary>
        /// <param name="postId">The post Id</param>
        /// <response code="200">User successfuly added to the list</response>
        /// <response code="400">Post does not exist or no more slots available for this event or user already applied to this event</response>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpPost(ApiRoutes.AttendedEvents.Attend)]
        public async Task<IActionResult> AttendEvent(int postId)
        {
            var user = await _userManager.FindByIdAsync(HttpContext.GetUserId());
            if (user == null)
                return BadRequest(new SingleError { Message = "The requester is not a registered user" });
            if (user.FirstName == null || user.LastName == null)
            {
                return BadRequest(new SingleError { Message = "User has to input first and last name in attend an event" });
            }
                     
            var post = await _postsRepository.GetPlainPostAsync(postId);
            if(post == null)
            {
                return NotFound();
            }
            if(post.UserId == user.Id)
            {
                return BadRequest(new SingleError { Message = "Cant join own event " });
            }
            var result = await _eventAttendanceService.AttendEvent(user, postId);

            if (!result.Result)
            {
                return BadRequest(new SingleError { Message = " No slots available / User already applied to this event / This event already passed" });
            }
            if (result.IsHouseParty)
            {
                await _notificationService.NotifyHostNewParticipationRequestAsync(new List<string> { result.HostId } , user.FirstName + " " + user.LastName );
            }

            return Ok();
        }


        /// <summary>
        /// This endpoint returns all events that the requester is going to participate in.
        /// The user data is retrieved from the JWT
        /// </summary>
        /// <response code="200">List of attended events or null if there are none</response>
        /// <response code="204">User is not attending any events.</response>
        [ProducesResponseType(typeof(Response<List<Bingo.Contracts.V1.Responses.Post.Posts>>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(Response<SingleError>), 400)]
        [HttpGet(ApiRoutes.AttendedEvents.GetActiveAttendedPosts)]
        public async Task<IActionResult> GetAllActiveAttendedEvents()
        {
            var user = await _userManager.FindByIdAsync(HttpContext.GetUserId());
            if (user == null)
                return BadRequest(new SingleError { Message = "The requester is not a registered user" });

            var result = await _eventAttendanceService.GetActiveAttendedPostsByUserId(user.Id);
            if (result.Count == 0)
            {
                return NoContent();
            }

            var resultList = new List<Bingo.Contracts.V1.Responses.Post.Posts>();

            foreach (var post in result)
            {
                var mappedPost = _domainToResponseMapper.MapPostForGetAllPostsReponse(post, _eventTypes);
                mappedPost.Slots = post.Event.GetSlotsIfAny();
                mappedPost.HostRating = await _ratingRepository.GetUserRating(post.UserId);
                resultList.Add(mappedPost);
            }

            return Ok(new Response<List<Bingo.Contracts.V1.Responses.Post.Posts>> { Data = resultList });
            //return Ok(new Response<List<ActiveAttendedEvent>> { Data = mapper.Map<List<ActiveAttendedEvent>>(result) });
        }



        /// <summary>
        /// This endpoint returns all events, that the requester attended in the past.
        /// The user data is retrieved from the JWT.
        /// </summary>
        /// <response code="200">Returns the list of events or null</response>
        /// <response code="204">User didn't attend any event so far.</response>
        [HttpGet(ApiRoutes.AttendedEvents.GetInactiveAttendedPosts)]
        [ProducesResponseType(typeof(Response<List<Bingo.Contracts.V1.Responses.Post.Posts>>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(Response<SingleError>), 400)]
        public async Task<IActionResult> GetAllOldAttendedEvents()
        {
            var user = await _userManager.FindByIdAsync(HttpContext.GetUserId());
            if (user == null)
                return BadRequest(new SingleError { Message = "The requester is not a registered user" });

            var result = await _eventAttendanceService.GetOldAttendedPostsByUserId(user.Id);
            if (result.Count==0)
            {
                return NoContent();
            }

            var resultList = new List<Bingo.Contracts.V1.Responses.Post.Posts>();

            foreach (var post in result)
            {
                var mappedPost = _domainToResponseMapper.MapPostForGetAllPostsReponse(post, _eventTypes);
                mappedPost.Slots = post.Event.GetSlotsIfAny();
                mappedPost.HostRating = await _ratingRepository.GetUserRating(post.UserId);
                resultList.Add(mappedPost);
            }

            return Ok(new Response<List<Bingo.Contracts.V1.Responses.Post.Posts>> { Data = resultList });
            //return Ok(new Response<List<ActiveAttendedEvent>> { Data = mapper.Map<List<ActiveAttendedEvent>>(result) });
        }

        

        /// <summary>
        /// This endpoint removes the requester from the participation list of an event
        /// The user data is retrieved from the JWT.
        /// </summary>
        /// <param name="postId">The post id</param>
        /// <response code="200">Removed</response>
        /// <response code="404">Post not found</response>
        /// <response code="400">Post does not exists/ user did not subscribe to this event</response>
        [HttpPost(ApiRoutes.AttendedEvents.UnAttend)]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(Response<SingleError>), 400)]
        public async Task<IActionResult> UnAttendEvent(int postId)
        {
            var user = await _userManager.FindByIdAsync(HttpContext.GetUserId());
            if (user == null)
                return BadRequest(new SingleError { Message = "The requester is not a registered user" });

            var post = await _postsRepository.GetPlainPostAsync(postId);
            if (post == null)
            {
                return NotFound();
            }

            var result = await _eventAttendanceService.UnAttendEvent(user, postId);

            if (!result)
            {
                return BadRequest(new SingleError { Message = "User did not applied to this event" });
            }
            return Ok();
        }
    }
}
