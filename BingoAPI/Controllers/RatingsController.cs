using AutoMapper;
using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.Rating;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.Rating;
using BingoAPI.Cache;
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
        private readonly IRatingRepository _ratingRepository;
        private readonly IUriService _uriService;
        private readonly IEventAttendanceRepository _attendanceRepository;
        private readonly IPostsRepository _postsRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public RatingsController(IRatingRepository ratingRepository, IUriService uriService,
                                 IEventAttendanceRepository attendanceRepository, IPostsRepository postsRepository,
                                 IMapper mapper, UserManager<AppUser> userManager)
        {
            this._ratingRepository = ratingRepository;
            this._uriService = uriService;
            this._attendanceRepository = attendanceRepository;
            this._postsRepository = postsRepository;
            this._mapper = mapper;
            this._userManager = userManager;
        }


        /// <summary>
        /// This endpoint returns a rating by Id.
        /// </summary>
        /// <param name="ratingId">The rating Id</param>
        /// <response code="200">Success</response>
        /// <response code="404">Rating not found</response>
        [ProducesResponseType(typeof(Response<GetRating>), 200)]
        [ProducesResponseType(typeof(SingleError),404)]
        [HttpGet(ApiRoutes.Ratings.Get)]
        public async Task<IActionResult> GetRating(int ratingId)
        {           
            var rating = await _ratingRepository.GetByIdAsync(ratingId);
            if (rating == null)
            {
                return NotFound(new SingleError { Message = "Rating not found" });
            }
            return Ok(new Response<GetRating>(_mapper.Map<GetRating>(rating)));
        }


        /// <summary>
        /// This endpoint returns all ratings of an user by his Id.
        /// </summary>
        /// <param name="userId">The user Id</param>
        /// <response code="200">Success</response>
        /// <response code="204">No ratings for this user yet</response>
        [ProducesResponseType(typeof(Response<List<GetRating>>), 200)]
        [ProducesResponseType(204)]
        [HttpGet(ApiRoutes.Ratings.GetAll)]
        [Cached(3600)]
        public async Task<IActionResult> GetRatings(string userId)
        {
            var result = await _ratingRepository.GetAllAsync(userId);
            if(result.Count == 0)
            {
                return NoContent();
            }

            return Ok(new Response<List<GetRating>>(_mapper.Map<List<GetRating>>(result)));
        }



        /// <summary>
        /// This endpoint is used for rating the host based on his event specified in the request. The rating is assigned to the host.
        /// The average rating of the host will be displayed in his events data.
        /// Ratings can be given only by users attending the event.
        /// </summary>
        /// <param name="createRequest">The rating data, the host id, the event id</param>
        /// <response code="201">Success</response>
        /// <response code="403">Requester is not attending this event / Provided user id is not the event host /
        /// Requester already rated this event</response>
        /// <response code="400">Rating could not be submitted</response>
        [ProducesResponseType(typeof(Response<CreateRatingResponse>), 201)]
        [ProducesResponseType(typeof(SingleError), 403)]
        [ProducesResponseType(typeof(SingleError), 400)]
        [HttpPost(ApiRoutes.Ratings.Create)]
        public async Task<IActionResult> CreateRating([FromBody] CreateRatingRequest createRequest)
        {
            // check if user is attending this event & accepted
            var requesterId = HttpContext.GetUserId();
            var isAttending = await _attendanceRepository.IsUserAttendingEvent(requesterId, createRequest.PostId);
            if (!isAttending)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "You do not attend this event/ not accepted" });
            }
           
            var hasAlreadyRated = await _ratingRepository.HasAlreadyRatedAsync(requesterId, createRequest.UserId, createRequest.PostId);
            if (hasAlreadyRated)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "User already rated Host for this event" });
            }

            Rating rating = _mapper.Map<Rating>(createRequest);
            rating.RaterId = requesterId;
            var result = await _ratingRepository.AddAsync(rating);
            if (!result)
            {
                return BadRequest(new SingleError { Message = "Rating could not be persisted" });
            }

            // TODO UriRatings
            var locationUri = _uriService.GetRatingUri(rating.Id.ToString());
            return Created(locationUri, new Response<CreateRatingResponse>(_mapper.Map<CreateRatingResponse>(rating)));
        }



        /// <summary>
        /// This endpoint is used for deleting a rating.
        /// Can be deleted only by admins
        /// </summary>
        /// <param name="ratingId">The rating Id</param>
        /// <response code="204">Successfully deleted</response>
        /// <response code="400">Delete failed / Rating did not exist</response>
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(SingleError), 400)]
        [HttpDelete(ApiRoutes.Ratings.Delete)]
        [Authorize(Roles ="Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteRating([FromRoute]int ratingId)
        {
            var result = await _ratingRepository.DeleteAsync(ratingId);
            if (!result)
            {
                return BadRequest(new SingleError { Message = "Rating could not be deleted / Did not exist" });
            }
            return NoContent();
        }


    }
}
