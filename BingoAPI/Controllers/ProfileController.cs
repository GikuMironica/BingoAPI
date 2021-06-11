using AutoMapper;
using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.Profile;
using BingoAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using BingoAPI.Models.SqlRepository;

namespace BingoAPI.Controllers
{

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Admin,User")]
    [Produces("application/json")]
    public class ProfileController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IRatingRepository _ratingRepository;

        public ProfileController(UserManager<AppUser> userManager, IMapper mapper, IRatingRepository _ratingRepository)
        {
            this._userManager = userManager;
            this._mapper = mapper;
            this._ratingRepository = _ratingRepository;
        }


        /// <summary>
        /// This endpoint returns the profile data of a user by his Id.
        /// Can be viewed by any authenticated app user.
        /// </summary>
        /// <param name="userId">The user Id</param>
        /// <response code="200">Success</response>
        /// <response code="404">User not found</response>
        [ProducesResponseType(typeof(Response<ProfileResponse>), 200)]
        [ProducesResponseType(404)]
        [HttpGet(ApiRoutes.Profile.Get)]
        public async Task<IActionResult> GetProfile([FromRoute] string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            var response = new Response<ProfileResponse>(_mapper.Map<ProfileResponse>(user))
            {
                Data = {Rating = await _ratingRepository.GetUserRating(user.Id)}
            };
            return Ok(response);
        }

    }
}
