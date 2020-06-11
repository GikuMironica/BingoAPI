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

        [Authorize(Roles ="SuperAdmin,Admin")]
        [HttpGet(ApiRoutes.Reports.Get)]
        public async Task<IActionResult> GetReport(int reportId)
        {
            Report report = await reportsRepository.GetByIdAsync(reportId);
            if(report == null)
            {
                return BadRequest(new SingleError { Message = "Report could not be found" });
            }

            return Ok(new Response<ReportResponse>(mapper.Map<ReportResponse>(report)));
        }

        
        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpGet(ApiRoutes.Reports.GetAll)]
        public async Task<IActionResult> GetReports(string userId)
        {
            List<Report> reports = await reportsRepository.GetAllAsync(userId);
            if(reports.Count == 0)
            {
                return Ok(new Response<String> { Data = "No Reports on this User" });
            }

            return Ok(new Response<List<ReportResponse>>(mapper.Map<List<ReportResponse>>(reports)));
        }


        [HttpPost(ApiRoutes.Reports.Create)]
        public async Task<IActionResult> CreateReport([FromBody] CreateReportRequest reportRequest)
        {
            var reporterId = HttpContext.GetUserId();
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


        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpDelete(ApiRoutes.Reports.Delete)]
        public async Task<IActionResult> DeleteReport(int reportId)
        {
            var result = await reportsRepository.DeleteAsync(reportId);
            if (!result)
            {
                return BadRequest(new SingleError { Message = "Report could not be deleted / Did not exist" });
            }
            return NoContent();
        }


        [Authorize(Roles = "SuperAdmin,Admin")]
        [HttpDelete(ApiRoutes.Reports.DeleteAll)]
        public async Task<IActionResult> DeleteAllReportForUser(string userId)
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
