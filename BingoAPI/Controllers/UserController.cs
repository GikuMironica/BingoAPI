using AutoMapper;
using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.User;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.User;
using BingoAPI.Cache;
using BingoAPI.Domain;
using BingoAPI.Extensions;
using BingoAPI.Helpers;
using BingoAPI.Models;
using BingoAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BingoAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Admin,User")]
    [Produces("application/json")]
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IUriService uriService;
        private readonly IAwsBucketManager awsBucketManager;
        private readonly IImageLoader imageLoader;

        public UserController(UserManager<AppUser> userManager,
                              IMapper mapper, IUriService uriService, IAwsBucketManager awsBucketManager,
                              IImageLoader imageLoader)
        {
            _userManager = userManager;
            _mapper = mapper;
            this.uriService = uriService;
            this.awsBucketManager = awsBucketManager;
            this.imageLoader = imageLoader;
        }

        /// <summary>
        /// Returns relevant data about all the users in the system
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "SuperAdmin, Admin")]
        [HttpGet(ApiRoutes.Users.GetAll)]
        public async Task<IActionResult> GetAll([FromQuery] PaginationQuery paginationQuery)
        {
            var paginationFilter = _mapper.Map<PaginationFilter>(paginationQuery);
            var users = await GetUsersAsync(paginationFilter);
            var userResponse = _mapper.Map<List<UserResponse>>(users);

            var paginationResponse = PaginationHelpers.CreatePaginatedResponse(uriService, paginationFilter, userResponse);

            return Ok(paginationResponse);
        }


        /// <summary>
        /// Returns relevant data about a system user by Id
        /// </summary>
        /// <param name="userId">Id of the user</param>
        /// <response code="200">Returns the updated user </response>
        /// <response code="404">user not found </response>
        [ProducesResponseType(typeof(Response<UserResponse>),200)]
        [HttpGet(ApiRoutes.Users.Get)]
        public async Task<IActionResult> Get([FromRoute] string userId)
        {
            var requesterId = HttpContext.GetUserId();
            var isOwnerOrAdmin = requesterId == userId;            
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();


            var requester = await _userManager.FindByIdAsync(requesterId);
            var requesterRoles = await _userManager.GetRolesAsync(requester);            

            foreach (var role in requesterRoles)
            {
                if (role == "Admin" || role == "SuperAdmin")
                    isOwnerOrAdmin = true;
            }

            if (!isOwnerOrAdmin)
            {
                return BadRequest(new SingleError { Message = "You do not own this account/ You are not an admin" });
            }

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
        [ProducesResponseType(406)]
        [ProducesResponseType(404)]
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



        [HttpPut(ApiRoutes.Users.UpdateProfilePicture)]
        public async Task<IActionResult> UpdatePicture([FromRoute] string userId, [FromForm] UpdateUserPictureRequest userPictureRequest)
        {
            // Compare the user id from the request & claim
            var verificationResult = await IsProfileOwnerOrAdminAsync(HttpContext.GetUserId(), userId);
            if (!verificationResult.Result)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "User can only update his own profile" });
            }

            await DeletePicturesAsync(verificationResult.User);            
            ImageProcessingResult imageProcessingResult = imageLoader.LoadFiles(new List<IFormFile> { userPictureRequest.UpdatedPicture });

            if (imageProcessingResult.Result)
            {
                imageProcessingResult.BucketPath = AwsAssetsPath.ProfilePictures;
                var uploadResult = await awsBucketManager.UploadFileAsync(imageProcessingResult);
                if (!uploadResult.Result)
                {
                    return BadRequest (new SingleError { Message = "The provided image couldn't be stored. Try to upload other picture." });
                }
                verificationResult.User.ProfilePicture = uploadResult.ImageNames.FirstOrDefault();
            }
            else { return BadRequest(new SingleError { Message = imageProcessingResult.ErrorMessage }); }


            var result = await _userManager.UpdateAsync(verificationResult.User);

            if (result.Succeeded)
            {
                return Ok(new Response<UserResponse>(_mapper.Map<UserResponse>(verificationResult.User)));
            }

            return BadRequest();
        }



        /// <summary>
        /// This endpoint allows users to delete their account 
        /// </summary>
        /// <param name="userId">The user id to be deleted</param>
        /// <response code="204">User successfuly deleted</response>
        /// <response code="403">Not enough priviledges</response>
        /// <response code="406">Unable to update user due to system requirements of the application user</response>
        [HttpDelete(ApiRoutes.Users.Delete)]
        public async Task<IActionResult> Delete([FromRoute] string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var deleted = await _userManager.DeleteAsync(user);
            if (deleted.Succeeded)
            {
                return NoContent();
            }                   

            return StatusCode(StatusCodes.Status406NotAcceptable, new SingleError { Message = "User can't be deleted due to some system constraints" });
        }

        public async Task<List<AppUser>> GetUsersAsync(PaginationFilter paginationFilter = null)
        {

            var queryable = _userManager.Users.AsQueryable();  

            if(paginationFilter == null)
            {
                paginationFilter = new PaginationFilter { PageNumber = 1, PageSize = 50 };
            }

            var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;

            return await queryable.Skip(skip).Take(paginationFilter.PageSize).ToListAsync();
        }

        public async Task<ProfileOwnershipState> IsProfileOwnerOrAdminAsync(string requesterId, string userId)
        {
            var isOwner = requesterId == userId;
            var user = await _userManager.FindByIdAsync(userId);
            if(user == null)
            {
                return new ProfileOwnershipState { Result = false };
            }
            var userRoles = await _userManager.GetRolesAsync(user);
            var isAdmin = false;

            foreach (var role in userRoles)
            {
                if (role == "Admin" || role == "SuperAdmin")
                    isAdmin = true;
            }


            return new ProfileOwnershipState { Result = isOwner || isAdmin, User = user };
        }

        private async Task DeletePicturesAsync(AppUser User)
        {           

            if (User.ProfilePicture != null)
            {
                var deletedPicturesResult = await awsBucketManager.DeleteFileAsync(new List<string> { User.ProfilePicture }, AwsAssetsPath.ProfilePictures);
                if (!deletedPicturesResult.Result)
                {
                    // log the Delete Exceptions list

                }
            }
        }

    }
}
