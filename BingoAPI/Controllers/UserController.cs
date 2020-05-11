using AutoMapper;
using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.User;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.User;
using BingoAPI.Cache;
using BingoAPI.Extensions;
using BingoAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin, Admin ,User")]
    [Produces("application/json")]
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        public UserController(UserManager<AppUser> userManager,
                              IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns relevant data about all the users in the system -> to be paginated.
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "SuperAdmin, Admin")]
        [HttpGet(ApiRoutes.Users.GetAll)]
        [Cached(600)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(new Response<List<UserResponse>>(_mapper.Map<List<UserResponse>>(_userManager.Users)));
        }


        /// <summary>
        /// Returns relevant data about a system user by Id
        /// </summary>
        /// <param name="userId">Id of the user</param>
        /// <response code="200">Returns the updated user </response>
        /// <response code="404">user not found </response>
        [ProducesResponseType(typeof(Response<UserResponse>),200)]
        [AllowAnonymous]
        [HttpGet(ApiRoutes.Users.Get)]
        [Cached(600)]
        public async Task<IActionResult> Get([FromRoute] string userId)
        {            
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            // domain to response contract mapping
            return Ok(new Response<UserResponse>(_mapper.Map<UserResponse>(user)));
        }


        /// <summary>
        /// Updates a user in the system,
        /// Users can update only their own profile
        /// </summary>
        /// <param name="userId">Id of the user</param>
        /// <param name="request">All these attributes will be updated</param>
        /// <response code="200">Returns the updated user</response>
        /// <response code="404">User not found</response>
        /// <response code="403">User can only update his own profile</response>
        /// <response code="400">Unable to update user due to invalid attributes of the model</response>
        /// <response code="406">Unable to update user due to system requirements of the application user</response>
        [ProducesResponseType(typeof(Response<UserResponse>), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(SingleError), 403)]
        [HttpPut(ApiRoutes.Users.Update)]
        public async Task<IActionResult> Update([FromRoute] string userId, [FromBody] UpdateUserRequest request)
        {
            // Compare the user id from the request & claim
            if (HttpContext.GetUserId() != userId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "User can only update his own profile" } );
            }

            var user = await _userManager.FindByIdAsync(userId);
            if(user == null)
            {
                return NotFound();
            }

            _mapper.Map<UpdateUserRequest, AppUser>(request, user);
              
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded) 
            {
                return Ok(new Response<UserResponse>(_mapper.Map<UserResponse>(user)));
            }

            return StatusCode(406);
        }



        /// <summary>
        /// Only administration can completely delete users. The users can only disable their accounts 
        /// </summary>
        /// <param name="userId">The user id to be deleted</param>
        /// <response code="204">User successfuly deleted</response>
        /// <response code="403">Not enough priviledges</response>
        /// <response code="406">Unable to update user due to system requirements of the application user</response>
        [HttpDelete(ApiRoutes.Users.Delete)]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> Delete([FromRoute] string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var deleted = await _userManager.DeleteAsync(user);
            if (deleted.Succeeded)
            {
                return NoContent();
            }                   

            return StatusCode(StatusCodes.Status406NotAcceptable, new SingleError { Message = "User can't be deleted due to some system constraints" });
        }
        
    }
}
