using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Requests;
using Bingo.Contracts.V1;
using BingoAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Controllers
{
    [Produces("application/json")]
    public class IdentityController : Controller
    {
        private readonly IIdentityService _identityService;
        public IdentityController(IIdentityService identityService)
        {
            this._identityService = identityService;
        }

        /// <summary>
        /// Registers user in the system
        /// </summary>
        /// <param name="request">Request object containing user email , password</param>
        /// <response code="200">Authentication result containing the jwt token and http response code</response>
        /// <response code="400">List of errors</response>
        [HttpPost(ApiRoutes.Identity.Register)]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthFailedResponse
                {
                    Errors = ModelState.Values.SelectMany(x => x.Errors.Select(xx => xx.ErrorMessage))
                });
            }

            // register the incoming user data with identity service
            var authResponse = await _identityService.RegisterAsync(request.Email, request.Password);
            if (!authResponse.Success)
            {
                return BadRequest(new AuthFailedResponse
                {
                    Errors = authResponse.Errors
                });
            }

            return Ok(new AuthSuccessResponse
            {
                Token = authResponse.Token,
                RefreshToken = authResponse.RefreshToken
            });
        }

        /// <summary>
        /// Logs in the user in the system
        /// </summary>
        /// <param name="request">Request containing user email and password</param>
        /// <response code="200">Authentication result containing the jwt token and http response code</response>
        /// <response code="400">List of errors</response>
        [HttpPost(ApiRoutes.Identity.Login)]
        public async Task<IActionResult> LoginAsync([FromBody] UserLoginRequest request)
        {
            var authResponse = await _identityService.LoginAsync(request.Email, request.Password);
            if (!authResponse.Success)
            {
                return BadRequest(new AuthFailedResponse
                {
                    Errors = authResponse.Errors
                });
            }
            return Ok(new AuthSuccessResponse
            {
                Token = authResponse.Token,
                RefreshToken = authResponse.RefreshToken
            });
        }


        /// <summary>
        /// Generates a new JWT, Refresh token combination and stores it 
        /// in the system databse
        /// </summary>
        /// <param name="request">Contains the JWT and the refresh token</param>
        /// <response code="200">New JWT, Refresh token combination</response>
        /// <response code="400">List of errors</response>
        [HttpPost(ApiRoutes.Identity.Refresh)]
        public async Task<IActionResult> RefreshAsync([FromBody] RefreshTokenRequest request)
        {
            var authResponse = await _identityService.RefreshTokenAsync(request.Token, request.RefreshToken);

            if (!authResponse.Success)
            {
                return BadRequest(new AuthFailedResponse
                {
                    Errors = authResponse.Errors
                });
            }

            return Ok(new AuthSuccessResponse
            {
                Token = authResponse.Token,
                RefreshToken = authResponse.RefreshToken
            });
       
        }
                
    }
}
