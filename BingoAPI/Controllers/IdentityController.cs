using Bingo.Contracts.V1.Responses.Identity;
using Bingo.Contracts.V1.Requests.Identity;
using Bingo.Contracts.V1;
using BingoAPI.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using BingoAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Bingo.Contracts.V1.Responses;
using BingoAPI.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using BingoAPI.Extensions;

namespace BingoAPI.Controllers
{
    [Produces("application/json")]
    public class IdentityController : Controller
    {
        private readonly IIdentityService _identityService;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IEmailFormatter _emailFormatter;
        private readonly IErrorService _errorService;
        private readonly String _createEditPostClaim = "post.add";

        public const string WebPortalRelativeUrl = "https://hopaut.com";
        public const string WebPortalResetPasswordConfirmationPage = WebPortalRelativeUrl + "/account/resetpassword";
        private const string WebPortalEmailConfirmationPage = WebPortalRelativeUrl + "/account/confirmemail";

        public IdentityController(IIdentityService identityService,
                                  UserManager<AppUser> userManager,
                                  SignInManager<AppUser> signInManager,
                                  IEmailService emailService,
                                  IEmailFormatter emailFormatter,
                                  IErrorService errorService)
        {
            this._identityService = identityService;
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._emailService = emailService;
            _emailFormatter = emailFormatter;
            _errorService = errorService;
        }

        /// <summary>
        /// Registers user in the system. User will be able to use the app only upon email confirmation.
        /// An email with a confirmation link will be sent to the user.
        /// </summary>
        /// <param name="request">Request object containing user email , password</param>
        /// <response code="200">If user registered, email confirmation link is sent to user over email, 200 ok returned with userId</response>
        /// <response code="400">List of errors</response>
        [ProducesResponseType(typeof(Response<string>), 200)]
        [ProducesResponseType(typeof(AuthFailedResponse), 400)]
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
            var authResponse = await _identityService.RegisterAsync(request.Email, request.Password, request.Language);          

            if (!authResponse.Success)
            {
                return BadRequest(new AuthFailedResponse
                {
                    Errors = authResponse.Errors
                });
            }
            
            // confirm registration
            return Ok(new Response<string> { Data = authResponse.UserId });
        }

        /// <summary>
        /// Logs in the user in the system
        /// Fail reasons: 0, 1, 2
        ///     0: Email not confirmed
        ///     1: Too many invalid attempts, account temporarly locked
        ///     2: Invalid password
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
            var authResponse = await _identityService.LoginAsync(request.Email, request.Password);
            if (!authResponse.Success)
            {
                return authResponse.FailReason != FailReason.InvalidPassword
                    ? StatusCode(StatusCodes.Status403Forbidden,
                        new AuthFailedResponse
                        {
                            FailReason = (int) authResponse.FailReason,
                            Errors = authResponse.Errors
                        })
                    : BadRequest(new AuthFailedResponse
                    {
                        FailReason = (int) authResponse.FailReason,
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
        /// <response code="403">Email not confirmed yet</response>
        /// <response code="400">Invalid token</response>
        [ProducesResponseType(typeof(AuthSuccessResponse), 200)]
        [ProducesResponseType(typeof(AuthFailedResponse), 400)]
        [HttpPost(ApiRoutes.Identity.FacebookAuth)]
        public async Task<IActionResult> Login([FromBody] UserFacebookAuthRequest request)
        {
            // authenticate the access token
            var authResponse = await _identityService.LoginWithFacebookAsync(request.AccessToken, request.Language);

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
        /// in the system database
        /// </summary>
        /// <param name="request">Contains the JWT and the refresh token</param>
        /// <response code="200">New JWT, Refresh token combination</response>
        /// <response code="400">List of errors</response>
        [ProducesResponseType(typeof(AuthSuccessResponse), 200)]
        [ProducesResponseType(typeof(AuthFailedResponse), 400)]
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
            return Redirect(WebPortalResetPasswordConfirmationPage);
        }


        /// <summary>
        /// This endpoint can be used by admins only, to confirm users email
        /// in case of system failure
        /// </summary>
        /// <param name="confirmEmailRequest">Email to be confirmed</param>
        /// <response code="200">Success</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Admin")]
        [ProducesResponseType(typeof(SingleError), 400)]
        [HttpPost(ApiRoutes.Identity.AdminConfirmEmail)]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest confirmEmailRequest)
        {
            string email = confirmEmailRequest.Email;
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return NotFound();
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            {
                return BadRequest(new SingleError { Message = "Email could not be confirmed" });
            }
            return Ok();
        }



        /// <summary>
        /// This endpoint send instructions to reset the password to the provided email address
        /// </summary>
        /// <param name="request">Contains the email of the account</param>
        /// <response code="200">The instructions have been successfully sent if the user is registered in the system</response>
        /// <response code="400">The provided email is not valid</response>
        [ProducesResponseType(typeof(Response<string>), 200)]
        [ProducesResponseType(typeof(SingleError), 400)]
        [HttpPost(ApiRoutes.Identity.ForgotPassword)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            // Filter middleware validates incoming model, if hits the line below, model state is valid
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)) )
            {
                // hide if user doesn't exist or not confirmed to avoid account enumeration
                return Ok(new Response<string> { Data = "If you are registered in our system, we have sent an email with the instructions to reset your password" });
            }

            // if user valid, generate token, send per email
            var authResponse = await _identityService.RequestNewPasswordAsync(user);

            if (!authResponse.Success)
            {
                return BadRequest(new SingleError
                {
                    Message = authResponse.Errors.FirstOrDefault()
                });
            }

            return Ok(new Response<string>
            {
                Data = "If you are registered in our system, we have sent an email with the instructions to reset your password"
            });

        }       


        /// <summary>
        /// When the user clicks the link in the email, new temporary password is set and send back to user's email
        /// </summary>
        /// <param name="request">Contains the email and the token</param>
        /// <response code="200"></response>
        [HttpGet(ApiRoutes.Identity.ResetPassword)]
        public async Task<IActionResult> ResetPassword([FromQuery] ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.email);
            
            if (user != null)
            {
                // TODO - Generate 8 symbol pass
                var upperchar = "ABCDEFGHIJKLMNOPQRSTUVWXYZqwertyuiopasdfghjklzxcvbnm1234567890";
                var pass = new string(
                    Enumerable.Repeat(upperchar, 8)
                    .Select(s => s[new Random().Next(upperchar.Length)])
                    .ToArray());

                var passResult = await _userManager.ResetPasswordAsync(user, request.token, pass);

                if (passResult.Succeeded)
                {
                    var content = _emailFormatter.FormatResetPassword(user.Language);
                    await _emailService.SendEmail(user.Email, content.EmailSubject, content.EmailContent.Replace("{GeneratedPassword}", pass));
                }                               
            }

            return Redirect(WebPortalResetPasswordConfirmationPage);
        }


        /// <summary>
        /// This end point changes the user's password
        /// </summary>
        /// <param name="request">Contains the old and the new password</param>
        /// <response code="200">Success confirmation</response>
        /// <response code="400">Provided password might not meet the security requirements</response>
        /// <response code="403">Email / Old password combination doesn't match</response>
        [ProducesResponseType(typeof(Response<string>), 200)]
        [ProducesResponseType(typeof(SingleError), 403)]
        [ProducesResponseType(typeof(AuthFailedResponse), 400)]
        [HttpPost(ApiRoutes.Identity.ChangePassword)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return Unauthorized(new SingleError { Message = "User Email / Old Password do not match" });
            }

            var authResponse = await _identityService.ChangePasswordAsync(user, request);

            if (!authResponse.Success)
            {
                return new BadRequestObjectResult(new AuthFailedResponse { Errors = authResponse.Errors } );
            }

            await _signInManager.RefreshSignInAsync(user);
            return Ok(new Response<string> { Data = "Password successfully update" });
        }



        /// <summary>
        /// This end point adds new password for current user if he has none ( in case he registered with facebook )
        /// </summary>
        /// <param name="request">Contains the new password</param>
        /// <response code="200">Success confirmation</response>
        /// <response code="400">Provided password might not meet the security requirements</response>
        [ProducesResponseType(typeof(Response<string>), 200)]
        [ProducesResponseType(typeof(AuthFailedResponse), 400)]
        [HttpPost(ApiRoutes.Identity.AddPassword)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Admin,User")]
        public async Task<IActionResult> AddPassword([FromBody] AddPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest();
            }
            var requester = await _userManager.FindByIdAsync(HttpContext.GetUserId());            
            if(user.Id != requester.Id)
            {
                return BadRequest();
            }
            var hasPassword = await _userManager.HasPasswordAsync(requester);
            if (hasPassword)
            {
                return BadRequest();
            }

            var authResponse = await _userManager.AddPasswordAsync(requester, request.NewPassword);
            if (!authResponse.Succeeded)
            {
                return BadRequest(new AuthFailedResponse { Errors = authResponse.Errors.Select(x => x.Description) });
            }

            await _signInManager.RefreshSignInAsync(user);
            return Ok(new Response<string> { Data = "Password successfully added" });
        }



        /// <summary>
        /// This endpoint disables the specified user's claim for creating posts. A time stamp is added in the claim value with the timestamp.
        /// </summary>
        /// <param name="request">The target user email</param>
        /// <response code="200">Action has been completed successfully</response>
        /// <response code="400">Action could not be completed. Check error logs</response>
        [HttpPost(ApiRoutes.Identity.RevokePostCreateClaim)]
        [ProducesResponseType(typeof(Response<SingleError>), 400)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> RevokeCreatePostClaim([FromBody] RevokePostClaimRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest(new SingleError
                {
                    Message = "User with this email does not exit"
                });
                
            }

            var claims = await _userManager.GetClaimsAsync(user);

            // find the post add/edit claim
            var postClaim = claims.FirstOrDefault(c => c.Type == _createEditPostClaim);

            var disabledClaim = new Claim(_createEditPostClaim, DateTimeOffset.UtcNow.ToLocalTime().ToUnixTimeSeconds().ToString());

            // replace the claim with another one, with the removal date timestamp as value
            var replaceClaimResult = await _userManager.ReplaceClaimAsync(user, postClaim, disabledClaim);
            if (!replaceClaimResult.Succeeded)
            {
                // logg
                var errorObj = new ErrorLog
                {
                    Date = DateTime.Now,
                    ExtraData = "Could not disable user's post add/edit claim. User email: "+user.Email,
                    Message = string.Join(">>next error>>", replaceClaimResult.Errors),
                    Controller = ControllerContext.ActionDescriptor.ControllerName
                };
                await _errorService.AddErrorAsync(errorObj);

                return BadRequest(new SingleError
                {
                    Message = "Could not disable the user's claim"
                });
            }

            return Ok();
        }



        /// <summary>
        /// This endpoint enables the specified user's claim for creating events. 
        /// </summary>
        /// <param name="request"></param>
        /// <response code="200">Action has been completed successfully</response>
        /// <response code="400">Action could not be completed. Check error logs</response>
        [HttpPost(ApiRoutes.Identity.GrantPostCreateClaim)]
        [ProducesResponseType(typeof(Response<SingleError>), 400)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GrantCreatePostClaim([FromBody] GrantPostClaimRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest(new SingleError
                {
                    Message = "User with this email does not exit"
                });
            }

            var claims = await _userManager.GetClaimsAsync(user);

            // find the post add/edit claim
            var postClaim = claims.FirstOrDefault(c => c.Type == _createEditPostClaim);

            var enabledClaim = new Claim(_createEditPostClaim, "true");

            // replace the claim with another one, with the value as true
            var replaceClaimResult = await _userManager.ReplaceClaimAsync(user, postClaim, enabledClaim);
            if (!replaceClaimResult.Succeeded)
            {
                // loggs
                var errorObj = new ErrorLog
                {
                    Date = DateTime.Now,
                    ExtraData = "Could not enable user's post add/edit claim. User email: " + user.Email,
                    Message = string.Join(">>next error>>", replaceClaimResult.Errors),
                    Controller = ControllerContext.ActionDescriptor.ControllerName
                };
                await _errorService.AddErrorAsync(errorObj);

                return BadRequest(new SingleError
                {
                    Message = "Could not disable the user's claim"
                });
            }

            return Ok();
        }


    }
}
