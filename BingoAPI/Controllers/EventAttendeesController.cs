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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BingoAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Admin,User")]
    [Produces("application/json")]
    public class EventAttendeesController : Controller
    {
        private readonly IEventParticipantsRepository _eventParticipantsRepository;
        private readonly IMapper _mapper;
        private readonly IUriService _uriService;
        private readonly INotificationService _notificationService;

        public EventAttendeesController(IEventParticipantsRepository eventParticipantsRepository, IMapper mapper,
                                        IUriService uriService, INotificationService notificationService)
        {
            this._eventParticipantsRepository = eventParticipantsRepository;
            this._mapper = mapper;
            this._uriService = uriService;
            this._notificationService = notificationService;
        }


        /// <summary>
        /// This endpoint enables the host to accept participants at his house party.
        /// Once an user is accepted, he will receive a push notification with the confirmation message.
        /// </summary>
        /// <param name="attendeeRequest">This object contains the post id and the requester id</param>
        /// <response code="200">Accepted</response>
        /// <response code="400">No slots available / user did not request to attent this party</response>
        /// <response code="403">The requester is not the event host</response>
        [HttpPost(ApiRoutes.EventAttendees.Accept)]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(SingleError), 400)]
        [ProducesResponseType(typeof(SingleError), 403)]
        public async Task<IActionResult> AcceptAttendee([FromBody] AttendeeRequest attendeeRequest)
        {
            if (!await IsOwner(attendeeRequest.PostId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "Requester is not the post owner or post does not exist" });
            }

            var result = await _eventParticipantsRepository.AcceptAttendee(attendeeRequest.AttendeeId, attendeeRequest.PostId);

            if (!result.Result)
            {
                return BadRequest(new SingleError { Message = "No slots available / user didn't request to attend this party / user already accepted" });
            }

            var userList = new List<string> { attendeeRequest.AttendeeId };
            await _notificationService.NotifyAttendEventRequestAcceptedAsync(userList, result.EventTitle, attendeeRequest.PostId);
            return Ok();
        }



        /// <summary>
        /// This endpoint rejects an user request to join a house party / removes a user from participators list.
        /// This endpoint is only accesible for the event hosts.
        /// </summary>
        /// <param name="attendeeRequest">This object contains the attendee Id, post id containing the event</param>
        /// <response code="200">Success</response>
        /// <response code="400">User is not in the participators list</response>
        /// <response code="403">Requester is not the event host</response>
        [ProducesResponseType(typeof(SingleError), 400)]
        [ProducesResponseType(typeof(SingleError), 403)]
        [ProducesResponseType(200)]
        [HttpPost(ApiRoutes.EventAttendees.Reject)]
        public async Task<IActionResult> RejectAttendee([FromBody] AttendeeRequest attendeeRequest)
        {
            if (!await IsOwner(attendeeRequest.PostId))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "Requester is not the post owner or post does not exist" });
            }

            var result = await _eventParticipantsRepository.RejectAttendee(attendeeRequest.AttendeeId, attendeeRequest.PostId);

            if (!result)
            {
                return BadRequest(new SingleError { Message = " user didn't request to attend this party" });
            }
            return Ok();
        }



        /// <summary>
        /// This endpoint fetches data about all event attendees regardless of whether they are accepted or in the pending list.
        /// This information is private and only accessible for the event host
        /// </summary>
        /// <param name="paginationQuery">Specifies the post id, pagination parameters, if not provided, the default is page 1, 50 results per page</param>
        /// <response code="200">Returns paginated result with the list of users.</response>
        /// <response code="204">Nobody attends this event or requested to attend it.</response>
        /// <response code="403">Requester is not the event host.</response>
        [HttpGet(ApiRoutes.EventAttendees.FetchAll)]
        [ProducesResponseType(typeof(PagedResponse<EventParticipant>), 200)]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(SingleError), 403)]
        public async Task<IActionResult> DisplayAll([FromQuery] PaginationQuery paginationQuery)
        {
            if (!await IsOwner(paginationQuery.Id))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "Requester is not the post owner or post does not exist" });
            }
            var paginationFilter = _mapper.Map<PaginationFilter>(paginationQuery);

            var participantsList = await _eventParticipantsRepository.DisplayAll(paginationQuery.Id, paginationFilter);
            if(participantsList.Count == 0)
            {
                return NoContent();
            }

            // map to response
            var eventParticipants = _mapper.Map<List<EventParticipant>>(participantsList);
            var paginationResponse = PaginationHelpers.CreatePaginatedResponse(_uriService, paginationFilter, eventParticipants);
            return Ok(paginationResponse);
        }



        /// <summary>
        /// This endpoint returns data about all users which are accepted in an event.
        /// This information is private and only accessible for the event host.
        /// </summary>
        /// <param name="paginationQuery">Specifies the post id, pagination parameters, if not provided, the default is page 1, 50 results per page</param>
        /// <response code="200">Returns paginated result with the list of users</response>
        /// <response code="204">No users are accepted to this event yet.</response>
        [ProducesResponseType(typeof(PagedResponse<EventParticipant>), 200)]
        [ProducesResponseType(204)]
        [HttpGet(ApiRoutes.EventAttendees.FetchAccepted)]
        public async Task<IActionResult> FetchAccepted([FromQuery] PaginationQuery paginationQuery)
        {            
            var paginationFilter = _mapper.Map<PaginationFilter>(paginationQuery);
            var participantsList = await _eventParticipantsRepository.DisplayAllAccepted(paginationQuery.Id, paginationFilter);
            if (participantsList.Count == 0)
            {
                return NoContent();
            }

            var eventParticipants = _mapper.Map<List<EventParticipant>>(participantsList);
            var paginationResponse = PaginationHelpers.CreatePaginatedResponse(_uriService, paginationFilter, eventParticipants);
            return Ok(paginationResponse);
        }



        /// <summary>
        /// This endpoint returns brief data about first 3 event attendees.
        /// </summary>
        /// <param name="fetchAttendees">Specifies the id of the post</param>
        /// <response code="200">Returns the data about first 3 attendees and total ammount of users</response>
        /// <response code="204">No users are accepted to this event yet.</response>
        [ProducesResponseType(typeof(Response<EventParticipantData>), 200)]
        [ProducesResponseType(204)]
        [HttpGet(ApiRoutes.EventAttendees.FetchAcceptedShort)]
        public async Task<IActionResult> FetchAcceptedShort(FetchAttendeesRequest fetchAttendees)
        {
            var participantsList = await _eventParticipantsRepository.DisplayShortlyAccepted(fetchAttendees.PostId);
            var number = await _eventParticipantsRepository.CountAccepted(fetchAttendees.PostId);
            if (participantsList.Count == 0)
            {
                return NoContent();
            }

            var result = new EventParticipantData
            {
                Attendees = _mapper.Map<List<EventParticipant>>(participantsList),
                AttendeesNumber = number
            };
            return Ok(new Response<EventParticipantData>(result));
        }




        /// <summary>
        /// This endpoint fetches the data about every user who requested to join this event.
        /// </summary>
        /// <param name="paginationQuery">Specifies the post id, pagination parameters, if not provided, the defaulti is page 1, 50 results per page</param>
        /// <response code="200">Returns paginated result with the list of users</response>
        /// <response code="204">No pending requests to join this event yet</response>
        [ProducesResponseType(typeof(PagedResponse<EventParticipant>), 200)]
        [ProducesResponseType(204)]
        [HttpGet(ApiRoutes.EventAttendees.FetchPending)]
        public async Task<IActionResult> FetchPending([FromQuery] PaginationQuery paginationQuery)
        {            
            if (!await IsOwner(paginationQuery.Id))
            {
                return BadRequest(new SingleError { Message = "Requester is not the post owner or post does not exist" });
            }
            var paginationFilter = _mapper.Map<PaginationFilter>(paginationQuery);
            var participantsList = await _eventParticipantsRepository.DisplayAllPending(paginationQuery.Id, paginationFilter);
            if (participantsList.Count == 0)
            {
                return NoContent();
            }

            var eventParticipants = _mapper.Map<List<EventParticipant>>(participantsList);
            var paginationResponse = PaginationHelpers.CreatePaginatedResponse(_uriService, paginationFilter, eventParticipants);
            return Ok(paginationResponse);
        }

        // TODO - refactor
        private async Task<bool> IsOwner(int postId)
        {
            var userId = HttpContext.GetUserId();
            var isPostOwner = await _eventParticipantsRepository.IsPostOwnerAsync(postId, userId);

            return isPostOwner;
        }
    }
}
