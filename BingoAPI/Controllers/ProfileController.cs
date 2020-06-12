using AutoMapper;
using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.Profile;
using BingoAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
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
    public class ProfileController : Controller
    {
        private readonly UserManager<AppUser> userManager;
        private readonly IMapper mapper;

        public ProfileController(UserManager<AppUser> userManager, IMapper mapper)
        {
            this.userManager = userManager;
            this.mapper = mapper;
        }


        /// <summary>
        /// This endoint returns the profile data of a user by his Id.
        /// Can be vieewed by any authenticated application member
        /// </summary>
        /// <param name="userId">The user Id</param>
        /// <response code="200">Success</response>
        /// <response code="404">User not found</response>
        [ProducesResponseType(typeof(Response<ProfileResponse>), 200)]
        [ProducesResponseType(typeof(SingleError), 403)]
        [HttpGet(ApiRoutes.Profile.Get)]
        public async Task<IActionResult> GetProfile([FromRoute] string userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound();

            return Ok(new Response<ProfileResponse>(mapper.Map<ProfileResponse>(user)));
        }

    }
}
