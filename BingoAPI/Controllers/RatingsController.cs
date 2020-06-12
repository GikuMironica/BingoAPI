using AutoMapper;
using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.Rating;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.Rating;
using BingoAPI.Extensions;
using BingoAPI.Helpers;
using BingoAPI.Models;
using BingoAPI.Models.SqlRepository;
using BingoAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Admin,User")]
    [Produces("application/json")]
    public class RatingsController : Controller
    {
        private readonly IRatingRepository ratingRepository;
        private readonly IUriService uriService;
        private readonly IEventAttendanceRepository attendanceRepository;
        private readonly IPostsRepository postsRepository;
        private readonly IMapper mapper;
        private readonly UserManager<AppUser> userManager;

        public RatingsController(IRatingRepository ratingRepository, IUriService uriService,
                                 IEventAttendanceRepository attendanceRepository, IPostsRepository postsRepository,
                                 IMapper mapper, UserManager<AppUser> userManager)
        {
            this.ratingRepository = ratingRepository;
            this.uriService = uriService;
            this.attendanceRepository = attendanceRepository;
            this.postsRepository = postsRepository;
            this.mapper = mapper;
            this.userManager = userManager;
        }


        /// <summary>
        /// This endoint returns a rating by Id.
        /// Can be vieewed by event admin / host
        /// </summary>
        /// <param name="ratingId">The rating Id</param>
        /// <response code="200">Success</response>
        /// <response code="403">Requester is not an admin or host either</response>
        /// <response code="404">Rating not found</response>
        [ProducesResponseType(typeof(Response<GetRating>), 200)]
        [ProducesResponseType(typeof(SingleError), 403)]
        [ProducesResponseType(typeof(SingleError),404)]
        [HttpGet(ApiRoutes.Ratings.Get)]
        public async Task<IActionResult> GetRating(int ratingId)
        {
            var requesterId = HttpContext.GetUserId();
            var requester = await userManager.FindByIdAsync(requesterId);
            var isRatingOwner = await ratingRepository.IsRatingOwnerAsync(requesterId, ratingId);
            var isAdmin = await RoleCheckingHelper.CheckIfAdmin(userManager, requester);

            if(!(isRatingOwner || isAdmin))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "Only user can view its own ratings or an admin" });
            }

            var rating = await ratingRepository.GetByIdAsync(ratingId);
            if (rating == null)
            {
                return NotFound(new SingleError { Message = "Rating not found" });
            }
            return Ok(new Response<GetRating>(mapper.Map<GetRating>(rating)));
        }



        /// <summary>
        /// This endoint returns all ratings of an user by his Id.
        /// Can be viewed by event admin / host
        /// </summary>
        /// <param name="userId">The user Id</param>
        /// <response code="200">Success</response>
        /// <response code="403">Requester is not an admin or host either</response>
        /// <response code="204">No ratings for this user yet</response>
        [ProducesResponseType(typeof(Response<List<GetRating>>), 200)]
        [ProducesResponseType(typeof(SingleError), 403)]
        [ProducesResponseType(204)]
        [HttpGet(ApiRoutes.Ratings.GetAll)]
        public async Task<IActionResult> GetRatings(string userId)
        {
            var requesterId = HttpContext.GetUserId();
            var requester = await userManager.FindByIdAsync(requesterId);
            var isRatingOwner = requesterId == userId;
            var isAdmin = await RoleCheckingHelper.CheckIfAdmin(userManager, requester);

            if (!(isRatingOwner || isAdmin))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "Only user can view its own ratings or an admin" });
            }

            var result = await ratingRepository.GetAllAsync(userId);
            if(result.Count == 0)
            {
                return NoContent();
            }

            return Ok(new Response<List<GetRating>>(mapper.Map<List<GetRating>>(result)));
        }



        /// <summary>
        /// This endoint is used for creating a rating.
        /// Only users attending an event can rate it
        /// </summary>
        /// <param name="createRequest">The rating data</param>
        /// <response code="201">Success</response>
        /// <response code="403">Requester is not attending this event / Provided user id is not event host / 
        /// requester already rated this event</response>
        /// <response code="400">Rating could not be submitted</response>
        [ProducesResponseType(typeof(Response<CreateRatingResponse>), 201)]
        [ProducesResponseType(typeof(SingleError), 403)]
        [ProducesResponseType(400)]
        [HttpPost(ApiRoutes.Ratings.Create)]
        public async Task<IActionResult> CreateRating([FromBody] CreateRatingRequest createRequest)
        {
            // check if user is attending this event & accepted
            var requesterId = HttpContext.GetUserId();
            var isAttending = await attendanceRepository.IsUserAttendingEvent(requesterId, createRequest.PostId);
            if (!isAttending)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "You do not attend this event/ not accepted" });
            }
            var isHostIdPostOwner = await postsRepository.IsHostIdPostOwner(createRequest.UserId, createRequest.PostId);
            if (!isHostIdPostOwner)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "User is not the host of this event" });
            }
            var hasAlreadyRated = await ratingRepository.HasAlreadyRatedAsync(requesterId, createRequest.UserId, createRequest.PostId);
            if (hasAlreadyRated)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "User already rated Host for this event" });
            }

            Rating rating = mapper.Map<Rating>(createRequest);
            rating.RaterId = requesterId;
            var result = await ratingRepository.AddAsync(rating);
            if (!result)
            {
                return BadRequest(new SingleError { Message = "Rating could not be persisted" });
            }

            // TODO UriRatings
            var locationUri = uriService.GetRatingUri(rating.Id.ToString());
            return Created(locationUri, new Response<CreateRatingResponse>(mapper.Map<CreateRatingResponse>(rating)));
        }



        /// <summary>
        /// This endoint is used for deleting a rating
        /// Can be deleted only by admins
        /// </summary>
        /// <param name="ratingId">The rating Id</param>
        /// <response code="204">Successfuly deleted</response>
        /// <response code="400">Delete failed / Rating did not exist</response>
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(SingleError), 400)]
        [HttpDelete(ApiRoutes.Ratings.Delete)]
        [Authorize(Roles ="Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteRating(int ratingId)
        {
            var result = await ratingRepository.DeleteAsync(ratingId);
            if (!result)
            {
                return BadRequest(new SingleError { Message = "Rating could not be deleted / Did not exist" });
            }
            return NoContent();
        }


    }
}
