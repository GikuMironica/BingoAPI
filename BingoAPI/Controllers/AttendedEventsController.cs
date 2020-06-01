using AutoMapper;
using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.AttendedEvent;
using BingoAPI.Extensions;
using BingoAPI.Models;
using BingoAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BingoAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin, Admin ,User")]
    [Produces("application/json")]
    public class AttendedEventsController : Controller
    {
        private readonly UserManager<AppUser> userManager;
        private readonly IEventAttendanceService eventAttendanceService;
        private readonly IMapper mapper;

        public AttendedEventsController(UserManager<AppUser> userManager, IEventAttendanceService eventAttendanceService,
                                        IMapper mapper)
        {
            this.userManager = userManager;
            this.eventAttendanceService = eventAttendanceService;
            this.mapper = mapper;
        }

        [HttpPost(ApiRoutes.AttendedEvents.Attend)]
        public async Task<IActionResult> AttendEvent(int postId)
        {
            var user = await userManager.FindByIdAsync(HttpContext.GetUserId());
            if (user == null)
                return BadRequest(new SingleError { Message = "The requester is not a registered user" });

            var result = await eventAttendanceService.AttendEvent(user, postId);

            if (!result)
            {
                return BadRequest(new SingleError { Message = "Post does not exist / No slots available / User already applied to this event" });
            }
            return Ok();
        }



        [HttpGet(ApiRoutes.AttendedEvents.GetActiveAttendedPosts)]
        public async Task<IActionResult> GetAllActiveAttendedEvents()
        {
            var user = await userManager.FindByIdAsync(HttpContext.GetUserId());
            if (user == null)
                return BadRequest(new SingleError { Message = "The requester is not a registered user" });

            var result = await eventAttendanceService.GetActiveAttendedPostsByUserId(user.Id);
            if(result == null)
            {
                return Ok("No events attended");
            }

            return Ok(new Response<List<ActiveAttendedEvent>> { Data = mapper.Map<List<ActiveAttendedEvent>>(result) });
        }




        [HttpGet(ApiRoutes.AttendedEvents.GetInactiveAttendedPosts)]
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


        [HttpPost(ApiRoutes.AttendedEvents.UnAttend)]
        public async Task<IActionResult> UnAttendEvent(int postId)
        {
            var user = await userManager.FindByIdAsync(HttpContext.GetUserId());
            if (user == null)
                return BadRequest(new SingleError { Message = "The requester is not a registered user" });

            var result = await eventAttendanceService.UnAttendEvent(user, postId);

            if (!result)
            {
                return BadRequest(new SingleError { Message = "Post does not exist / No slots available / User already applied to this event" });
            }
            return Ok();
        }
    }
}
