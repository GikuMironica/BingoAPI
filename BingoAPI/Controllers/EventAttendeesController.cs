using AutoMapper;
using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.EventAttendee;
using Bingo.Contracts.V1.Requests.User;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.EventAttendee;
using BingoAPI.Domain;
using BingoAPI.Extensions;
using BingoAPI.Helpers;
using BingoAPI.Models.SqlRepository;
using BingoAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BingoAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Admin,User")]
    [Produces("application/json")]
    public class EventAttendeesController : Controller
    {
        private readonly IEventParticipantsRepository eventParticipantsRepository;
        private readonly IMapper mapper;
        private readonly IUriService uriService;
        private readonly INotificationService notificationService;
        private readonly IPostsRepository postsRepository;

        public EventAttendeesController(IEventParticipantsRepository eventParticipantsRepository, IMapper mapper,
                                        IUriService uriService, INotificationService notificationService,
                                        IPostsRepository postsRepository)
        {
            this.eventParticipantsRepository = eventParticipantsRepository;
            this.mapper = mapper;
            this.uriService = uriService;
            this.notificationService = notificationService;
            this.postsRepository = postsRepository;
        }

        /// <summary>
        /// This endpoint accepts request of user to attend house party
        /// </summary>
        /// <param name="attendeeRequest">This object containts the post id and the requester id</param>
        /// <response code="200">Accepted</response>
        /// <response code="400">No slots available / user did not request to attent this party</response>
        [HttpPost(ApiRoutes.EventAttendees.Accept)]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(SingleError), 400)]
        public async Task<IActionResult> AcceptAttendee([FromForm] AttendeeRequest attendeeRequest)
        {
            if (!await IsOwner(attendeeRequest.PostId))
            {
                return BadRequest(new SingleError { Message = "Requester is not the post owner or post does not exist" });
            }

            var result = await eventParticipantsRepository.AcceptAttendee(attendeeRequest.AttendeeId, attendeeRequest.PostId);

            if (!result.Result)
            {
                return BadRequest(new SingleError { Message = "No slots available / user didn't request to attend this party / user already accepted" });
            }

            var userList = new List<string> { attendeeRequest.AttendeeId };
            await notificationService.NotifyAttendEventRequestAcceptedAsync(userList, result.EventTitle);

            return Ok();
        }



        /// <summary>
        /// This endpoint rejects the request to join a house party / removes a user from participators list.
        /// </summary>
        /// <param name="attendeeRequest">This object containes the attendee Id, post id containing the event</param>
        /// <response code="200">Rejected/Removed</response>
        /// <response code="400">User is not in the participators list</response>
        [ProducesResponseType(typeof(SingleError), 400)]
        [ProducesResponseType(200)]
        [HttpPost(ApiRoutes.EventAttendees.Reject)]
        public async Task<IActionResult> RejectAttendee([FromForm] AttendeeRequest attendeeRequest)
        {
            if (!await IsOwner(attendeeRequest.PostId))
            {
                return BadRequest(new SingleError { Message = "Requester is not the post owner or post does not exist" });
            }

            var result = await eventParticipantsRepository.RejectAttendee(attendeeRequest.AttendeeId, attendeeRequest.PostId);

            if (!result)
            {
                return BadRequest(new SingleError { Message = " user didn't request to attend this party" });
            }
            return Ok();
        }



        /// <summary>
        /// This endpoint fetches data about all event attendees or pending requests to attend a party
        /// </summary>
        /// <param name="paginationQuery">Specifies the pagination parameters, if not provided, the defaulti is page 1, 50 results per page</param>
        /// <param name="postId">The post id containing the event</param>
        /// <response code="200">Returns paginated result with the list of users</response>
        [HttpGet(ApiRoutes.EventAttendees.FetchAll)]
        [ProducesResponseType(typeof(PagedResponse<EventParticipant>), 200)]
        public async Task<IActionResult> DisplayAll([FromQuery] PaginationQuery paginationQuery, int postId)
        {
            if (!await IsOwner(postId))
            {
                return BadRequest(new SingleError { Message = "Requester is not the post owner or post does not exist" });
            }
            var paginationFilter = mapper.Map<PaginationFilter>(paginationQuery);

            var ParticipantsList = await eventParticipantsRepository.DisplayAll(postId, paginationFilter);

            // map to response
            var eventParticipants = mapper.Map<List<EventParticipant>>(ParticipantsList);
            var paginationResponse = PaginationHelpers.CreatePaginatedResponse(uriService, paginationFilter, eventParticipants);
            return Ok(paginationResponse);
        }



        /// <summary>
        /// This endpoint fetches data about all event accepted attendees
        /// </summary>
        /// <param name="paginationQuery">Specifies the pagination parameters, if not provided, the defaulti is page 1, 50 results per page</param>
        /// <param name="postId">The post id containing the event</param>
        /// <response code="200">Returns paginated result with the list of users</response>
        [ProducesResponseType(typeof(PagedResponse<EventParticipant>), 200)]
        [HttpGet(ApiRoutes.EventAttendees.FetchAccepted)]
        public async Task<IActionResult> FetchAccepted([FromQuery] PaginationQuery paginationQuery, int postId)
        {
            var eType = await postsRepository.GetEventType(postId);
            if (!await IsOwner(postId) && eType == 1)
            {
                return BadRequest(new SingleError { Message = "Requester is not the post owner or post does not exist" });
            }
            var paginationFilter = mapper.Map<PaginationFilter>(paginationQuery);
            var ParticipantsList = await eventParticipantsRepository.DisplayAllAccepted(postId, paginationFilter);

            var eventParticipants = mapper.Map<List<EventParticipant>>(ParticipantsList);
            var paginationResponse = PaginationHelpers.CreatePaginatedResponse(uriService, paginationFilter, eventParticipants);
            return Ok(paginationResponse);
        }



        /// <summary>
        /// This endpoint fetches data about all event pending request from attendees to attend an event
        /// </summary>
        /// <param name="paginationQuery">Specifies the pagination parameters, if not provided, the defaulti is page 1, 50 results per page</param>
        /// <param name="postId">The post id containing the event</param>
        /// <response code="200">Returns paginated result with the list of users</response>
        [ProducesResponseType(typeof(PagedResponse<EventParticipant>), 200)]
        [HttpGet(ApiRoutes.EventAttendees.FetchPending)]
        public async Task<IActionResult> FetchPending([FromQuery] PaginationQuery paginationQuery, int postId)
        {
           
            if (!await IsOwner(postId))
            {
                return BadRequest(new SingleError { Message = "Requester is not the post owner or post does not exist" });
            }
            var paginationFilter = mapper.Map<PaginationFilter>(paginationQuery);
            var ParticipantsList = await eventParticipantsRepository.DisplayAllPending(postId, paginationFilter);

            var eventParticipants = mapper.Map<List<EventParticipant>>(ParticipantsList);
            var paginationResponse = PaginationHelpers.CreatePaginatedResponse(uriService, paginationFilter, eventParticipants);
            return Ok(paginationResponse);
        }

        private async Task<bool> IsOwner(int postId)
        {
            var userId = HttpContext.GetUserId();
            var isPostOwner = await eventParticipantsRepository.IsPostOwnerAsync(postId, userId);

            if (!isPostOwner)
            {
                return false;
            }
            return true;
        }
    }
}
