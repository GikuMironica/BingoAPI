using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Responses;
using BingoAPI.Data;
using BingoAPI.Extensions;
using BingoAPI.Models;
using BingoAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BingoAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Admin,User")]
    [Produces("application/json")]
    public class ErrorController : Controller
    {
        
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly ILogger<ErrorController> logger;
        private readonly IErrorService errorService;

        public ErrorController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
            ILogger<ErrorController> logger, IErrorService errorService)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
            this.errorService = errorService;
        }

        [HttpGet(ApiRoutes.Error.ErrorRoute)]
        public async Task<IActionResult> Error()
        {
            // Retrieve the exception Details
            var exceptionHandlerPathFeature =
                HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            // LogError() method logs the exception under Error category in the log
            logger.LogError($"The path {exceptionHandlerPathFeature.Path} " +
                $"threw an exception {exceptionHandlerPathFeature.Error}");
            var execptionPath = exceptionHandlerPathFeature.Path;
            var Exmsg = exceptionHandlerPathFeature.Error.Message;
            var stack = exceptionHandlerPathFeature.Error.StackTrace;
            var innerEx = exceptionHandlerPathFeature.Error.InnerException;

            ErrorLog errorLog = new ErrorLog
            {
                UserId = HttpContext.GetUserId(),
                ActionMethod = execptionPath,
                Controller = execptionPath,
                Url = execptionPath,
                Message = Exmsg,
                Date = DateTime.Now
            };

            var result = await errorService.AddErrorAsync(errorLog);

            return BadRequest(new SingleError { Message = "Internal Error"});
        }


        [HttpGet("/swagger/{statuscode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            var statusCodeResult =
                HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            switch (statusCode)
            {
                case 404:
                    return NotFound();
            }

            return BadRequest();
        }

        [HttpGet("/swaggerr/index.html")]
        public IActionResult SIndex(int statusCode)
        {           

            return NotFound();
        }
    }
}
