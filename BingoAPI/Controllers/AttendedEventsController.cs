using AutoMapper;
using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.AttendedEvent;
using BingoAPI.Extensions;
using BingoAPI.Models;
using BingoAPI.Models.SqlRepository;
using BingoAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        public AttendedEventsController(UserManager<AppUser> userManager, IEventAttendanceRepository eventAttendanceService,
                                        IMapper mapper, INotificationService notificationService, IPostsRepository postsRepository)
        {
            this.userManager = userManager;
            this.eventAttendanceService = eventAttendanceService;
            this.mapper = mapper;
            this.notificationService = notificationService;
            this.postsRepository = postsRepository;
        }


        /// <summary>
        /// This end point add the user to the event participators list.
        /// If the event is a house party, it is added to the pending requests list
        /// </summary>
        /// <param name="postId">The post Id which has the event</param>
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
         //   if (user.FirstName == null || user.LastName == null)
         //   {
         //       return BadRequest(new SingleError { Message = "User has to input first and last name in attend an event" });
         //   }

            var result = await eventAttendanceService.AttendEvent(user, postId);
            var post = await postsRepository.GetPlainPostAsync(postId);
            if(post == null)
            {
                return NotFound();
            }

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
        /// This endpoint returns all events which user marked as "Attend" which are currently active
        /// </summary>
        /// <response code="200">List of attended events or null if there are none</response>
        [ProducesResponseType(typeof(Response<List<ActiveAttendedEvent>>), 200)]
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
                return Ok(new Response<string> { Data = "No events attended" } );
            }

            return Ok(new Response<List<ActiveAttendedEvent>> { Data = mapper.Map<List<ActiveAttendedEvent>>(result) });
        }



        /// <summary>
        /// This endpoint returns all events atttended by the user in the past
        /// </summary>
        /// <response code="200">Returns the list of events or null</response>
        [HttpGet(ApiRoutes.AttendedEvents.GetInactiveAttendedPosts)]
        [ProducesResponseType(typeof(Response<List<ActiveAttendedEvent>>), 200)]
        public async Task<IActionResult> GetAllOldAttendedEvents()
        {
            var user = await userManager.FindByIdAsync(HttpContext.GetUserId());
            if (user == null)
                return BadRequest(new SingleError { Message = "The requester is not a registered user" });

            var result = await eventAttendanceService.GetOldAttendedPostsByUserId(user.Id);
            if (result == null)
            {
                return Ok("No events attended");
            }

            return Ok(new Response<List<ActiveAttendedEvent>> { Data = mapper.Map<List<ActiveAttendedEvent>>(result) });
        }


        /// <summary>
        /// This endpoint removes user from the participation list of an event
        /// </summary>
        /// <param name="postId">The post id contained the event</param>
        /// <response code="200">Removed</response>
        /// <response code="400">Post does not exists/ user did not subscribe to this event</response>
        [HttpPost(ApiRoutes.AttendedEvents.UnAttend)]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Response<SingleError>), 200)]
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
