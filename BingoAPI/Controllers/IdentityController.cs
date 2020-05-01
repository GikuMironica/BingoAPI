using Bingo.Contracts.V1.Responses.Identity;
using Bingo.Contracts.V1.Requests.Identity;
using Bingo.Contracts.V1;
using BingoAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using BingoAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Bingo.Contracts.V1.Responses;
using Microsoft.AspNetCore.Http;

namespace BingoAPI.Controllers
{
    [Produces("application/json")]
    public class IdentityController : Controller
    {
        private readonly IIdentityService _identityService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;

        public IdentityController(IIdentityService identityService,
                                  UserManager<AppUser> userManager,
                                  IEmailService emailService)
        {
            this._identityService = identityService;
            this._userManager = userManager;
            this._emailService = emailService;
        }

        /// <summary>
        /// Registers user in the system
        /// </summary>
        /// <param name="request">Request object containing user email , password</param>
        /// <response code="200">If user registered, email confirmation link is sent to user over email, 200 ok returned</response>
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

            // confirm registration
            return Ok();
        }

        /// <summary>
        /// Logs in the user in the system
        /// </summary>
        /// <param name="request">Request containing user email and password</param>
        /// <response code="200">Authentication result containing the jwt token and http response code</response>
        /// <response code="400">List of errors</response>
        /// <response code="403">Email not confirmed</response>
        [ProducesResponseType(typeof(AuthFailedResponse), 403)]
        [ProducesResponseType(typeof(AuthFailedResponse), 400)]
        [ProducesResponseType(typeof(AuthSuccessResponse), 200)]
        [HttpPost(ApiRoutes.Identity.Login)]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user != null && !user.EmailConfirmed)                    
            {
                return StatusCode(StatusCodes.Status403Forbidden, new AuthFailedResponse
                {
                    Errors = new List<string> { "Email not confirmed yet" }
                });
            }
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
        /// This method registers/logs user in with facebook credentials
        /// </summary>
        /// <param name="request">Generated on client side with facebook sdk</param>
        /// <response code="200">Token validated, AuthenticationResponse returned with the jwt and refresh token</response>
        /// <response code="400">Invalid token</response>
        [ProducesResponseType(typeof(AuthSuccessResponse), 200)]
        [ProducesResponseType(typeof(AuthFailedResponse), 400)]
        [HttpPost(ApiRoutes.Identity.FacebookAuth)]
        public async Task<IActionResult> Login([FromBody] UserFacebookAuthRequest request)
        {
            // authenticate the access token
            var authResponse = await _identityService.LoginWithFacebookAsync(request.AccessToken);

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
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
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

        /// <summary>
        /// This method handles the user email confirmation upon registration,
        /// when the user hits the confirmation link, the user's email is marked as confirmed in the system 
        /// and confirmation email is sent
        /// </summary>
        /// <param name="userId">The registered user Id</param>
        /// <param name="token">The generated token - lifespan 1 day</param>
        /// <response code="200">Confirmed</response>
        /// <response code="400">Denied, token invalid or user does not exist</response>
        [HttpGet(ApiRoutes.Identity.ConfirmEmail)]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return BadRequest();
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
               await _emailService.SendEmail(user.Email, "BingoApp - Successfully Registered", "Congratulations,\n You have successfully activated your account!\n " +
                    "Welcome to the dark side.");
            }

            return Ok();
        }
                
    }
}
