using AutoMapper;
using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.User;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.User;
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin, Admin ,User")]
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


        [Authorize(Roles = "SuperAdmin, Admin, User")]
        [HttpGet(ApiRoutes.Users.GetAll)]
        public async Task<IActionResult> GetAll()
        {

            return Ok(_userManager.Users);
        }

        [HttpGet(ApiRoutes.Users.Get)]
        public async Task<IActionResult> Get([FromRoute] string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
                return NotFound();

            // domain to response contract mapping
            return Ok(new Response<UserResponse>(_mapper.Map<UserResponse>(user)));
        }

        [HttpPut(ApiRoutes.Users.Update)]
        public async Task<IActionResult> Update([FromRoute] string Id, [FromBody] UpdateUserRequest request)
        {
            return null;
        }

        [HttpDelete(ApiRoutes.Users.Delete)]
        public async Task<IActionResult> Delete([FromRoute] string Id)
        {
            return null;
        }
        
    }
}
