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
using System.Threading.Tasks;

namespace BingoAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Admin,User")]
    [Produces("application/json")]
    public class TagController : Controller
    {
        private readonly ITagsRepository _tagsRepository;
        private readonly IMapper _mapper;

        public TagController(ITagsRepository tagsRepository, IMapper mapper)
        {
            this._tagsRepository = tagsRepository;
            this._mapper = mapper;
        }



        [Cached(86400)]
        [HttpGet(ApiRoutes.Tag.GetAll)]
        public async Task<IActionResult> FindTags([FromRoute] GetAllTagsRequest tagsRequest)
        {
            var result = await _tagsRepository.FindTags(tagsRequest.TagName);
            if (result.Count == 0)
            {
                return NoContent();
            }

            return Ok(new Response<Tags>(_mapper.Map<Tags>(result)));
        }

    }
}
