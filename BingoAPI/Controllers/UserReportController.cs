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
        private readonly IUserReportRepository userReportRepository;
        private readonly IMapper mapper;
        private readonly IUriService uriService;

        public UserReportController(IUserReportRepository userReportRepository, IMapper mapper,
                                    IUriService uriService)
        {
            this.userReportRepository = userReportRepository;
            this.mapper = mapper;
            this.uriService = uriService;
        }




        /// <summary>
        /// This endoint returns a user report by Id.
        /// Can be viewed by administration only
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
            UserReport report = await userReportRepository.GetByIdAsync(reportId);
            if (report == null)
            {
                return NotFound(new SingleError { Message = "Report could not be found" });
            }

            return Ok(new Response<UserReportResponse>(mapper.Map<UserReportResponse>(report)));
        }



        /// <summary>
        /// This endoint returns all reports of an user by his Id.
        /// Can be viewed by event administration only
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
            var reports = await userReportRepository.GetAllAsync(userId);
            if (reports.Count == 0)
            {
                return NoContent();
            }


            return Ok(new Response<List<UserReportResponse>>(mapper.Map<List<UserReportResponse>>(reports)));
        }



        /// <summary>
        /// This endoint is used for reporting a user.
        /// Everyone can report a user
        /// </summary>
        /// <param name="reportUser">The report data</param>
        /// <response code="201">Success</response>
        /// <response code="403">Requester already reported this user, cooldown 1 week</response>
        /// <response code="400">Report could not be submitted</response>
        [ProducesResponseType(typeof(Response<ReportUserRequest>), 201)]
        [ProducesResponseType(typeof(SingleError), 403)]
        [ProducesResponseType(typeof(SingleError), 400)]
        [HttpPost(ApiRoutes.UserReports.Create)]
        public async Task<IActionResult> CreateReport([FromBody] ReportUserRequest reportUser)
        {
            var reporterId = HttpContext.GetUserId();
            if(!(await userReportRepository.CanReport(reporterId, reportUser.ReportedUserId)))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "Reporter already reported this User, cooldown 1 week" });
            }
            UserReport userReport = mapper.Map<UserReport>(reportUser);
            userReport.ReporterId = reporterId;
            var result = await userReportRepository.AddAsync(userReport);

            if (!result)
            {
                return BadRequest(new SingleError { Message = "Report could not be submitted" });
            }

            var locationUri = uriService.GetUserReportUri(userReport.Id.ToString());
            return Created(locationUri, new Response<CreateUserReportResponse>(mapper.Map<CreateUserReportResponse>(userReport)));
        }



        /// <summary>
        /// This endoint is used for deleting a report on a user
        /// Can be deleted only by admins
        /// </summary>
        /// <param name="reportId">The report Id</param>
        /// <response code="204">Successfuly deleted</response>
        /// <response code="400">Delete failed / Report did not exist</response>
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(SingleError), 400)]
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpDelete(ApiRoutes.UserReports.Delete)]
        public async Task<IActionResult> DeleteReport(int reportId)
        {
            var result = await userReportRepository.DeleteAsync(reportId);
            if (!result)
            {
                return BadRequest(new SingleError { Message = "Report could not be deleted / Did not exist" });
            }
            return NoContent();
        }

                
    }
}
