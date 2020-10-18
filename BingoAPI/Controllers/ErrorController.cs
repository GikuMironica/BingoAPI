using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Responses;
using BingoAPI.Extensions;
using BingoAPI.Models;
using BingoAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BingoAPI.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Admin,User")]
    [Produces("application/json")]
    public class ErrorController : Controller
    {
        /*private readonly ILogger<ErrorController> _logger;
        private readonly IErrorService _errorService;

        public ErrorController(ILogger<ErrorController> logger, IErrorService errorService)
        {
            this._logger = logger;
            this._errorService = errorService;
        }

        [HttpGet(ApiRoutes.Error.ErrorRoute)]
        public async Task<IActionResult> Error()
        {
            var errorLog = GetErrorLog();
            await _errorService.AddErrorAsync(errorLog);
            return BadRequest(new SingleError { Message = "Error"});
        }

        
        [HttpGet("/Error/{statuscode}")]
        public async Task<IActionResult> HttpStatusCodeHandler(int statusCode)
        {
            var statusCodeResult =
                HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            switch (statusCode)
            {
                case 404:
                    return NotFound();
                default:
                    var errorLog = GetErrorLog(statusCode);
                    await _errorService.AddErrorAsync(errorLog);
                    break;
            }
            return BadRequest();
        }
        
        private ErrorLog GetErrorLog(int statuscode = 0)
        {
            // Retrieve the exception Details
            var exceptionHandlerPathFeature =
                HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            // LogError() method logs the exception under Error category in the log
            _logger.LogError($"The path {exceptionHandlerPathFeature.Path} " +
                            $"threw an exception {exceptionHandlerPathFeature.Error}");
            var exceptionPath = exceptionHandlerPathFeature.Path;
            var exceptionMessage = exceptionHandlerPathFeature.Error.Message;

            ErrorLog errorLog = new ErrorLog
            {
                UserId = HttpContext.GetUserId(),
                ActionMethod = exceptionPath,
                Controller = exceptionPath,
                Url = exceptionPath,
                Message = exceptionMessage,
                Date = DateTime.Now,
                ExtraData = ("Status Code: "+ statuscode)
            };
            return errorLog;
        }
        /*
         [HttpGet("/swagger/index.html")]
         public IActionResult S1Index()
         {           
             return NotFound();
         }

        [HttpGet("/swagger/v1/swagger.json")]
        public IActionResult S2Index()
        {           
            return NotFound();
        }#1#

    }*/
    }
}
