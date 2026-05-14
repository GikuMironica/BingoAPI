using AutoMapper;
using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.UserReport;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.UserReport;
using BingoAPI.Extensions;
using BingoAPI.Models;
using BingoAPI.Models.SqlRepository;
using BingoAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    public class UserReportController : Controller
    {
        private readonly IUserReportRepository _userReportRepository;
        private readonly IMapper _mapper;
        private readonly IUriService _uriService;
        private readonly UserManager<AppUser> _userManager;

        public UserReportController(IUserReportRepository userReportRepository, IMapper mapper,
                                    IUriService uriService, UserManager<AppUser> userManager)
        {
            this._userReportRepository = userReportRepository;
            this._mapper = mapper;
            this._uriService = uriService;
            this._userManager = userManager;
        }




        /// <summary>
        /// This endpoint returns the report on an user, by the report Id.
        /// Can be viewed by administration only.
        /// </summary>
        /// <param name="reportId">The report Id</param>
        /// <response code="200">Success</response>
        /// <response code="404">Report not found</response>
        [ProducesResponseType(typeof(Response<UserReportResponse>), 200)]
        [ProducesResponseType(typeof(SingleError), 404)]
        [Authorize(Roles ="Admin,SuperAdmin")]
        [HttpGet(ApiRoutes.UserReports.Get)]
        public async Task<IActionResult> GetReport(int reportId)
        {
            UserReport report = await _userReportRepository.GetByIdAsync(reportId);
            if (report == null)
            {
                return NotFound(new SingleError { Message = "Report could not be found" });
            }

            return Ok(new Response<UserReportResponse>(_mapper.Map<UserReportResponse>(report)));
        }



        /// <summary>
        /// This endpoint returns all reports on an user by his Id.
        /// Can be viewed by event administration only.
        /// </summary>
        /// <param name="userId">The user Id</param>
        /// <response code="200">Success</response>
        /// <response code="204">No reports on this user yet</response>
        [ProducesResponseType(typeof(Response<List<UserReportResponse>>), 200)]
        [ProducesResponseType(204)]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet(ApiRoutes.UserReports.GetAll)]
        public async Task<IActionResult> GetReports(string userId)
        {
            var reports = await _userReportRepository.GetAllAsync(userId);
            if (reports.Count == 0)
            {
                return NoContent();
            }


            return Ok(new Response<List<UserReportResponse>>(_mapper.Map<List<UserReportResponse>>(reports)));
        }



        /// <summary>
        /// This endpoint is used for reporting an user.
        /// Everyone can report a user.
        /// </summary>
        /// <param name="reportUser">The report data</param>
        /// <response code="201">Success</response>
        /// <response code="403">Requester already reported this user, cooldown 1 week</response>
        /// <response code="400">Report could not be submitted</response>
        /// <response code="404">User not found</response>
        [ProducesResponseType(typeof(Response<ReportUserRequest>), 201)]
        [ProducesResponseType(typeof(SingleError), 403)]
        [ProducesResponseType(typeof(SingleError), 400)]
        [ProducesResponseType(404)]
        [HttpPost(ApiRoutes.UserReports.Create)]
        public async Task<IActionResult> CreateReport([FromBody] ReportUserRequest reportUser)
        {
            var reporterId = HttpContext.GetUserId();
            var reported = await _userManager.FindByIdAsync(reportUser.ReportedUserId);
            if(reported == null)
            {
                return NotFound();
            }

            if(!(await _userReportRepository.CanReport(reporterId, reportUser.ReportedUserId)))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "Reporter already reported this User, cooldown 1 week" });
            }
            UserReport userReport = _mapper.Map<UserReport>(reportUser);
            userReport.ReporterId = reporterId;
            var result = await _userReportRepository.AddAsync(userReport);

            if (!result)
            {
                return BadRequest(new SingleError { Message = "Report could not be submitted" });
            }

            var locationUri = _uriService.GetUserReportUri(userReport.Id.ToString());
            return Created(locationUri, new Response<CreateUserReportResponse>(_mapper.Map<CreateUserReportResponse>(userReport)));
        }



        /// <summary>
        /// This endpoint is used for deleting a report on an user.
        /// Can be deleted only by admins.
        /// </summary>
        /// <param name="reportId">The report Id</param>
        /// <response code="204">Successfully deleted</response>
        /// <response code="400">Delete failed / Report did not exist</response>
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(SingleError), 400)]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpDelete(ApiRoutes.UserReports.Delete)]
        public async Task<IActionResult> DeleteReport([FromRoute]int reportId)
        {
            if(reportId == 0)
            {
                return BadRequest();
            }
            var result = await _userReportRepository.DeleteAsync(reportId);
            if (!result)
            {
                return BadRequest(new SingleError { Message = "Report could not be deleted / Did not exist" });
            }
            return NoContent();
        }

                
    }
}
