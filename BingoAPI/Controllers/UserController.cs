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
using System.Security.Cryptography.Xml;
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
        /// Returns relevant data about all the users in the system.
        /// Only administration has access to this resource.
        /// </summary>
        /// <param name="paginationQuery">Contains the pagination configuration like page nr, page size. Default values are 1 and 50.</param>
        /// <response code="200">Returns paginated response with all users</response>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Admin")]
        [ProducesResponseType(typeof(PagedResponse<UserResponse>), 200)]
        [HttpGet(ApiRoutes.Users.GetAll)]
        public async Task<IActionResult> GetAll([FromQuery] UsersPaginationQuery paginationQuery)
        {
            var paginationFilter = _mapper.Map<PaginationFilter>(paginationQuery);
            var users = await GetUsersAsync(paginationFilter);
            var userResponse = _mapper.Map<List<UserResponse>>(users);

            var paginationResponse = PaginationHelpers.CreatePaginatedResponse(uriService, paginationFilter, userResponse);

            return Ok(paginationResponse);
        }


        /// <summary>
        /// Returns relevant data about a system user by Id.
        /// This data is only available to admins and the account owner.
        /// </summary>
        /// <param name="userId">Id of the user</param>
        /// <response code="200">Returns the data</response>
        /// <response code="404">User not found </response>
        [ProducesResponseType(typeof(Response<UserResponse>),200)]
        [ProducesResponseType(typeof(SingleError),403)]
        [ProducesResponseType(404)]
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
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "You do not own this account/ You are not an admin" });
            }

            // domain to response contract mapping
            return Ok(new Response<UserResponse>(_mapper.Map<UserResponse>(user)));
        }


        /// <summary>
        /// Updates a user in the system.
        /// The data can be updated only by the account owner or an admin.
        /// </summary>
        /// <param name="userId">Id of the user</param>
        /// <param name="request">All attributes that are not null, will be updated</param>
        /// <response code="200">Returns the updated user</response>
        /// <response code="404">User not found</response>
        /// <response code="403">User can only update his own profile</response>
        /// <response code="400">Attempt to input invalid data</response>
        /// <response code="406">Unable to update user due to system requirements of the application user</response>
        [ProducesResponseType(typeof(Response<UserResponse>), 200)]
        [ProducesResponseType(406)]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(SingleError), 403)]
        [HttpPut(ApiRoutes.Users.Update)]
        public async Task<IActionResult> Update([FromRoute] string userId, [FromBody] UpdateUserRequest request)
        {
            // Compare the user id from the request & claim
            var verificationResult = await IsProfileOwnerOrAdminAsync(HttpContext.GetUserId(), userId);
            if (!verificationResult.Result)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "User can only update his own profile" });
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
        /// This endpoint is used for updating the profile picture of an user.
        /// This data can only be updated by the account owner or an admin.
        /// </summary>
        /// <param name="userId">The account Id</param>
        /// <param name="userPictureRequest">Contains the new profile picture</param>
        /// <response code="403">User can only update his own profile</response>
        /// <response code="400">Provided image could not be persited.</response>
        /// <response code="200">Returns the users data.</response>
        [ProducesResponseType(typeof(Response<UserResponse>), 200)]
        [ProducesResponseType(typeof(SingleError), 400)]
        [HttpPut(ApiRoutes.Users.UpdateProfilePicture)]
        public async Task<IActionResult> UpdatePicture([FromRoute] string userId, [FromForm] UpdateUserPictureRequest userPictureRequest)
        {
            // Compare the user id from the request & claim
            var verificationResult = await IsProfileOwnerOrAdminAsync(HttpContext.GetUserId(), userId);
            if (!verificationResult.Result)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "User can only update his own profile" });
            }

            // delete existing pictures
            await DeletePicturesAsync(verificationResult.User);     
            
            // load pictures in memory stream
            ImageProcessingResult imageProcessingResult = await imageLoader.LoadFiles(new List<IFormFile> { userPictureRequest.UpdatedPicture });

            // upload to bucket
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
        /// <response code="400">Bad request</response>
        /// <response code="406">Unable to update user due to system requirements of the application user</response>
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        [ProducesResponseType(typeof(SingleError), 406)]
        [HttpDelete(ApiRoutes.Users.Delete)]
        public async Task<IActionResult> Delete([FromRoute] string userId)
        {
            // Compare the user id from the request & claim
            var verificationResult = await IsProfileOwnerOrAdminAsync(HttpContext.GetUserId(), userId);
            if (!verificationResult.Result)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "User can only update his own profile" });
            }

            var deleted = await _userManager.DeleteAsync(verificationResult.User);
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
            var requester = await _userManager.FindByIdAsync(requesterId);
            if(user == null)
            {
                return new ProfileOwnershipState { Result = false };
            }
            var userRoles = await _userManager.GetRolesAsync(requester);
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
