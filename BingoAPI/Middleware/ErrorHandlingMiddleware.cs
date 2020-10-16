using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using BingoAPI.Extensions;
using BingoAPI.Models;
using BingoAPI.Services;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace BingoAPI.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IErrorService _errorService;

        public ErrorHandlingMiddleware(RequestDelegate next, IErrorService errorService)
        {
            _next = next;
            _errorService = errorService;
        }

        public async Task Invoke(HttpContext context, IWebHostEnvironment env)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var error = await HandleExceptionAsync(context, ex, env);
            }
        }

        private async Task<ErrorLog> HandleExceptionAsync(HttpContext context, Exception exception, IWebHostEnvironment env)
        {
            var stackTrace = String.Empty;
            var status = HttpStatusCode.InternalServerError;
            string message = "Server-side error";
            var exceptionPath = context.Request.Path;

            if (env.IsEnvironment("Development"))
            {
                stackTrace = exception.StackTrace;
                message = exception.Message;
            }

            ErrorLog errorLog = new ErrorLog
            {
                UserId = context.GetUserId(),
                ActionMethod = exceptionPath,
                Controller = exceptionPath,
                Message = exception.Message,
                Date = DateTime.Now,
                ExtraData = exception.StackTrace
            };

          
            await _errorService.AddErrorAsync(errorLog);
            var result = JsonConvert.SerializeObject(new { error = message});
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)status;
            await context.Response.WriteAsync(result);

            return errorLog;
        }
    }
}
    