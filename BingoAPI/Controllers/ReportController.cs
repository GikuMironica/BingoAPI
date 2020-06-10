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

        public ReportController(IUriService uriService, IReportsRepository reportsRepository,
                                IMapper mapper)
        {
            this.uriService = uriService;
            this.reportsRepository = reportsRepository;
            this.mapper = mapper;
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
        public async Task<IActionResult> GetReports(int userId)
        {
            
            return Ok();
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
            var result = await reportsRepository.AddAsync(report);
            if (!result)
            {
                return BadRequest(new SingleError { Message = "Report could not be submitted" });
            }
            var locationUri = uriService.GetReportUri(report.Id.ToString());
            return Created(locationUri, new Response<CreateReportResponse>(mapper.Map<CreateReportResponse>(report)));
        }


        [HttpDelete(ApiRoutes.Reports.Delete)]
        public async Task<IActionResult> DeleteReport(int reportId)
        {

            return NoContent();
        }

    }
}
