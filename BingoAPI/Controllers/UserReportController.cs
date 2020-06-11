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


        [Authorize(Roles ="Admin,SuperAdmin")]
        [HttpGet(ApiRoutes.UserReports.Get)]
        public async Task<IActionResult> GetReport(int reportId)
        {
            UserReport report = await userReportRepository.GetByIdAsync(reportId);
            if (report == null)
            {
                return BadRequest(new SingleError { Message = "Report could not be found" });
            }

            return Ok(new Response<UserReportResponse>(mapper.Map<UserReportResponse>(report)));
        }


        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet(ApiRoutes.UserReports.GetAll)]
        public async Task<IActionResult> GetReports(string userId)
        {
            var reports = await userReportRepository.GetAllAsync(userId);
            if (reports.Count == 0)
            {
                return BadRequest(new SingleError { Message = "No reports on this user" });
            }


            return Ok(new Response<List<UserReportResponse>>(mapper.Map<List<UserReportResponse>>(reports)));
        }


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
