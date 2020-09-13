using AutoMapper;
using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.AttendedEvent;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Admin,User")]
    [Produces("application/json")]
    public class AttendedEventsController : Controller
    {
        private readonly UserManager<AppUser> userManager;
        private readonly IEventAttendanceRepository eventAttendanceService;
        private readonly IMapper mapper;
        private readonly INotificationService notificationService;
        private readonly IPostsRepository postsRepository;
        private readonly IDomainToResponseMapper domainToResponseMapper;
        private readonly IRatingRepository ratingRepository;
        private readonly EventTypes eventTypes;

        public AttendedEventsController(UserManager<AppUser> userManager, IEventAttendanceRepository eventAttendanceService,
                                        IMapper mapper, INotificationService notificationService, IPostsRepository postsRepository,
                                        IDomainToResponseMapper domainToResponseMapper, IOptions<EventTypes> eventTypes, IRatingRepository ratingRepository)
        {
            this.userManager = userManager;
            this.eventAttendanceService = eventAttendanceService;
            this.mapper = mapper;
            this.notificationService = notificationService;
            this.postsRepository = postsRepository;
            this.domainToResponseMapper = domainToResponseMapper;
            this.ratingRepository = ratingRepository;
            this.eventTypes = eventTypes.Value;
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
            var user = await userManager.FindByIdAsync(HttpContext.GetUserId());
            if (user == null)
                return BadRequest(new SingleError { Message = "The requester is not a registered user" });
            if (user.FirstName == null || user.LastName == null)
            {
                return BadRequest(new SingleError { Message = "User has to input first and last name in attend an event" });
            }
                     
            var post = await postsRepository.GetPlainPostAsync(postId);
            if(post == null)
            {
                return NotFound();
            }
            if(post.UserId == user.Id)
            {
                return BadRequest(new SingleError { Message = "Cant join own event " });
            }
            var result = await eventAttendanceService.AttendEvent(user, postId);

            if (!result.Result)
            {
                return BadRequest(new SingleError { Message = " No slots available / User already applied to this event / This event already passed" });
            }
            if (result.IsHouseParty)
            {
                await notificationService.NotifyHostNewParticipationRequestAsync(new List<string> { result.HostId } , user.FirstName + " " + user.LastName );
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
            var user = await userManager.FindByIdAsync(HttpContext.GetUserId());
            if (user == null)
                return BadRequest(new SingleError { Message = "The requester is not a registered user" });

            var result = await eventAttendanceService.GetActiveAttendedPostsByUserId(user.Id);
            if (result.Count == 0)
            {
                return NoContent();
            }

            var resultList = new List<Bingo.Contracts.V1.Responses.Post.Posts>();

            foreach (var post in result)
            {
                var mappedPost = domainToResponseMapper.MapPostForGetAllPostsReponse(post, eventTypes);
                mappedPost.Slots = post.Event.GetSlotsIfAny();
                mappedPost.HostRating = await ratingRepository.GetUserRating(post.UserId);
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
            var user = await userManager.FindByIdAsync(HttpContext.GetUserId());
            if (user == null)
                return BadRequest(new SingleError { Message = "The requester is not a registered user" });

            var result = await eventAttendanceService.GetOldAttendedPostsByUserId(user.Id);
            if (result.Count==0)
            {
                return NoContent();
            }

            var resultList = new List<Bingo.Contracts.V1.Responses.Post.Posts>();

            foreach (var post in result)
            {
                var mappedPost = domainToResponseMapper.MapPostForGetAllPostsReponse(post, eventTypes);
                mappedPost.Slots = post.Event.GetSlotsIfAny();
                mappedPost.HostRating = await ratingRepository.GetUserRating(post.UserId);
                resultList.Add(mappedPost);
            }

            return Ok(new Response<List<Bingo.Contracts.V1.Responses.Post.Posts>> { Data = resultList });
            //return Ok(new Response<List<ActiveAttendedEvent>> { Data = mapper.Map<List<ActiveAttendedEvent>>(result) });
        }



        /// <summary>
        /// This endpoint returns all events that the user will attend or has attended already, and that contains any announcements.
        /// The returned list of posts is sorted descending by the timestamp of their last announcement. 
        /// i.e the post with the most recent announcement comes on top of the list.
        /// </summary>
        /// <response code="200">Returns a custom mini post for announcement data.</response>
        /// <response code="204">No announcements to display</response>
        [ProducesResponseType(typeof(Response<List<MiniPostForAnnouncements>>), 200)]
        [ProducesResponseType(204)]
        [HttpGet(ApiRoutes.AttendedEvents.GetAllWithAnnouncements)]
        public async Task<IActionResult> GetAllWithAnnouncements()
        {
            var user = await userManager.FindByIdAsync(HttpContext.GetUserId());
            if (user == null)
                return BadRequest(new SingleError { Message = "The requester is not a registered user" });

            var result = await eventAttendanceService.GetAttendedEventsWithAnnouncements(user.Id);

            if (result.Count == 0)
                return NoContent();

            var resultList = new List<MiniPostForAnnouncements>();
            foreach (var post in result)
            {
                var mappedPost = domainToResponseMapper.MapMiniPostForAnnouncementsList(post, eventTypes);
                resultList.Add(mappedPost);
            }

            return Ok(new Response<List<MiniPostForAnnouncements>> { Data = resultList });
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
            var user = await userManager.FindByIdAsync(HttpContext.GetUserId());
            if (user == null)
                return BadRequest(new SingleError { Message = "The requester is not a registered user" });

            var post = await postsRepository.GetPlainPostAsync(postId);
            if (post == null)
            {
                return NotFound();
            }

            var result = await eventAttendanceService.UnAttendEvent(user, postId);

            if (!result)
            {
                return BadRequest(new SingleError { Message = "User did not applied to this event" });
            }
            return Ok();
        }
    }
}
