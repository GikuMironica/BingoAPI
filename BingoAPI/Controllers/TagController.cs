using AutoMapper;
using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.Tag;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.Tag;
using BingoAPI.Cache;
using BingoAPI.Models.SqlRepository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Admin,User")]
    [Produces("application/json")]
    public class TagController : Controller
    {
        private readonly ITagsRepository tagsRepository;
        private readonly IMapper mapper;

        public TagController(ITagsRepository tagsRepository, IMapper mapper)
        {
            this.tagsRepository = tagsRepository;
            this.mapper = mapper;
        }



        [Cached(86400)]
        [HttpGet(ApiRoutes.Tag.GetAll)]
        public async Task<IActionResult> FindTags([FromRoute] GetAllTagsRequest tagsRequest)
        {
            var result = await tagsRepository.FindTags(tagsRequest.TagName);
            if (result.Count == 0)
            {
                return NoContent();
            }

            return Ok(new Response<Tags>(mapper.Map<Tags>(result)));
        }

    }
}
