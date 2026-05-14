using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.Bug;
using Bingo.Contracts.V1.Responses;
using BingoAPI.Domain;
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
    public class BugReportController : Controller
    {
        private readonly IBugReportRepository _bugReportRepository;
        private readonly IImageLoader _imageLoader;
        private readonly IAwsBucketManager _awsBucketManager;

        public BugReportController(IBugReportRepository bugReportRepository, IImageLoader imageLoader, IAwsBucketManager awsBucketManager)
        {
            _bugReportRepository = bugReportRepository;
            _imageLoader = imageLoader;
            _awsBucketManager = awsBucketManager;
        }

        /// <summary>
        /// This endpoint is used for reporting a post.
        /// Everyone can report a post.
        /// </summary>
        /// <param name="reportRequest">The report data. The message maximum length is 300 characters with at most 3 pictures.</param>
        /// <response code="200">Success</response>
        /// <response code="400">Report could not be submitted</response>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(SingleError), 400)]
        [HttpPost(ApiRoutes.BugReports.Create)]
        public async Task<IActionResult> CreateReport([FromForm] CreateBugReport reportRequest)
        {
            var reporterId = HttpContext.GetUserId();

            var bugReport = new Bug { BugScreenshots = new List<BugScreenshot>() };
            var imagesProcessingResult = await ProcessImagesAsync(reportRequest.Screenshots, bugReport);
            if (!imagesProcessingResult.Result) { return BadRequest(new SingleError { Message = imagesProcessingResult.ErrorMessage }); }

            var result = await _bugReportRepository.AddAsync(bugReport);
            if (!result)
            {
                return BadRequest(new SingleError { Message = "Report could not be submitted" });
            }
            return Ok();
        }

        private async Task<ImageProcessingResult> ProcessImagesAsync(IList<IFormFile> screenshots, Bug bug)
        {
            if ((screenshots?.Count > 0))
            {
                ImageProcessingResult imageProcessingResult = await _imageLoader.LoadFiles(screenshots.ToList());

                if (imageProcessingResult.Result)
                {
                    imageProcessingResult.BucketPath = AwsAssetsPath.BugScreenshots;
                    var uploadResult = await _awsBucketManager.UploadFileAsync(imageProcessingResult);
                    if (!uploadResult.Result)
                    {
                        return new ImageProcessingResult { Result = false, ErrorMessage = "Reason_3, The provided images couldn't be stored. Try to upload other pictures." };
                    }
                    foreach (var uploadedPic in uploadResult.ImageNames)
                    {
                        bug.BugScreenshots.Add(new BugScreenshot
                        {
                            Url = uploadedPic
                        });
                    }
                }
                else { return new ImageProcessingResult { Result = false, ErrorMessage = imageProcessingResult.ErrorMessage }; }
            }
            return new ImageProcessingResult { Result = true };
        }
    }
}
