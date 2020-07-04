using AutoMapper;
using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.Report;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.Report;
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
using System.Threading.Tasks;

namespace BingoAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Admin,User")]
    [Produces("application/json")]
    public class ReportController : Controller
    {
        private readonly IUriService uriService;
        private readonly IReportsRepository reportsRepository;
        private readonly IMapper mapper;
        private readonly IPostsRepository postRepository;

        public ReportController(IUriService uriService, IReportsRepository reportsRepository,
                                IMapper mapper, IPostsRepository postRepository)
        {
            this.uriService = uriService;
            this.reportsRepository = reportsRepository;
            this.mapper = mapper;
            this.postRepository = postRepository;
        }



        /// <summary>
        /// This endpoint returns a report on a post by Id.
        /// Can be viewed by administration only.
        /// </summary>
        /// <param name="reportId">The report Id</param>
        /// <response code="200">Success</response>
        /// <response code="404">Report not found</response>
        [ProducesResponseType(typeof(Response<ReportResponse>), 200)]
        [ProducesResponseType(typeof(SingleError), 404)]
        [Authorize(Roles ="SuperAdmin,Admin")]
        [HttpGet(ApiRoutes.Reports.Get)]
        public async Task<IActionResult> GetReport(int reportId)
        {
            Report report = await reportsRepository.GetByIdAsync(reportId);
            if(report == null)
            {
                return NotFound(new SingleError { Message = "Report could not be found" });
            }

            return Ok(new Response<ReportResponse>(mapper.Map<ReportResponse>(report)));
        }



        /// <summary>
        /// This endpoint returns all reports on users posts by his Id.
        /// Can be viewed by administration only.
        /// </summary>
        /// <param name="userId">The user Id</param>
        /// <response code="200">Success</response>
        /// <response code="204">No reports on this user yet</response>
        [ProducesResponseType(typeof(Response<List<ReportResponse>>), 200)]
        [ProducesResponseType(204)]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet(ApiRoutes.Reports.GetAll)]
        public async Task<IActionResult> GetReports(string userId)
        {
            List<Report> reports = await reportsRepository.GetAllAsync(userId);
            if(reports.Count == 0)
            {
                return NoContent();
            }

            return Ok(new Response<List<ReportResponse>>(mapper.Map<List<ReportResponse>>(reports)));
        }




        /// <summary>
        /// This endpoint is used for reporting a post.
        /// Everyone can report a post.
        /// </summary>
        /// <param name="reportRequest">The report data. The message minimum length is 10 characters.</param>
        /// <response code="201">Success</response>
        /// <response code="403">User already reported this event</response>
        /// <response code="400">Report could not be submitted</response>
        [ProducesResponseType(typeof(Response<CreateReportResponse>), 201)]
        [ProducesResponseType(typeof(SingleError), 403)]
        [ProducesResponseType(typeof(SingleError), 400)]
        [HttpPost(ApiRoutes.Reports.Create)]
        public async Task<IActionResult> CreateReport([FromBody] CreateReportRequest reportRequest)
        {
            var reporterId = HttpContext.GetUserId();

            // if post exists
            var post = await postRepository.GetPlainPostAsync(reportRequest.PostId);
            if (post == null)
            {
                return BadRequest(new SingleError { Message = "Post does not exist" });
            }

            var hasAlreadyReported = await reportsRepository.HasAlreadyReported(reporterId, reportRequest.PostId);
            if (hasAlreadyReported)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "User already reported this event" });
            }

            Report report = mapper.Map<Report>(reportRequest);
            report.ReporterId = reporterId;
            report.ReportedHostId = await postRepository.GetHostId(reportRequest.PostId);
            var result = await reportsRepository.AddAsync(report);
            if (!result)
            {
                return BadRequest(new SingleError { Message = "Report could not be submitted" });
            }
            var locationUri = uriService.GetReportUri(report.Id.ToString());
            return Created(locationUri, new Response<CreateReportResponse>(mapper.Map<CreateReportResponse>(report)));
        }



        /// <summary>
        /// This endpoint is used for deleting a report on a post.
        /// Can be deleted only by admins
        /// </summary>
        /// <param name="reportId">The report Id</param>
        /// <response code="204">Successfuly deleted</response>
        /// <response code="400">Delete failed / Report did not exist</response>
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(SingleError), 400)]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpDelete(ApiRoutes.Reports.Delete)]
        public async Task<IActionResult> DeleteReport([FromRoute]int reportId)
        {
            var result = await reportsRepository.DeleteAsync(reportId);
            if (!result)
            {
                return BadRequest(new SingleError { Message = "Report could not be deleted / Did not exist" });
            }
            return NoContent();
        }



        /// <summary>
        /// This endpoint is used for deleting all reports on a users posts.
        /// Can be deleted only by admins
        /// </summary>
        /// <param name="userId">The user Id</param>
        /// <response code="204">Successfuly deleted</response>
        /// <response code="400">Delete failed / Reports did not exist</response>
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(SingleError), 400)]
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpDelete(ApiRoutes.Reports.DeleteAll)]
        public async Task<IActionResult> DeleteAllReportForUser([FromRoute]string userId)
        {
            var result = await reportsRepository.DeleteAllForUserAsync(userId);
            if (!result)
            {
                return BadRequest(new SingleError { Message = "Reports could not be deleted / No reports on this user" });
            }
            return NoContent();
        }
    }
}
