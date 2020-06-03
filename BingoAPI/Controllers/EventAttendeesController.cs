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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin, Admin, User")]
    [Produces("application/json")]
    public class EventAttendeesController : Controller
    {
        private readonly IEventParticipantsRepository eventParticipantsRepository;
        private readonly IMapper mapper;
        private readonly IUriService uriService;

        public EventAttendeesController(IEventParticipantsRepository eventParticipantsRepository, IMapper mapper,
                                        IUriService uriService)
        {
            this.eventParticipantsRepository = eventParticipantsRepository;
            this.mapper = mapper;
            this.uriService = uriService;
        }

        
        [HttpPost(ApiRoutes.EventAttendees.Accept)]
        public async Task<IActionResult> AcceptAttendee([FromForm] AttendeeRequest attendeeRequest)
        {
            if (!await IsOwner(attendeeRequest.PostId))
            {
                return BadRequest(new SingleError { Message = "Requester is not the post owner or post does not exist" });
            }

            var result = await eventParticipantsRepository.AcceptAttendee(attendeeRequest.AttendeeId, attendeeRequest.PostId);

            if (!result)
            {
                return BadRequest(new SingleError { Message = "No slots available / user didn't request to attend this party" });
            }

            return Ok();

        }


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
                return BadRequest(new SingleError { Message = "No slots available / user didn't request to attend this party" });
            }
            return Ok();
        }



        [HttpGet(ApiRoutes.EventAttendees.FetchAll)]
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



        public async Task<IActionResult> FetchAccepted()
        {
            return Ok();
        }



        public async Task<IActionResult> FetchPending()
        {
            return Ok();
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
