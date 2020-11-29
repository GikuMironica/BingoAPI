using AutoMapper;
using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.Announcement;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.Announcement;
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
using System.Collections.Generic;
using System.Threading.Tasks;
using Bingo.Contracts.V1.Responses.AttendedEvent;
using BingoAPI.CustomMapper;
using BingoAPI.Domain;
using BingoAPI.Options;
using Microsoft.Extensions.Options;

namespace BingoAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Admin,User")]
    [Produces("application/json")]
    public class AnnouncementController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly EventTypes _eventTypes;
        private readonly IPostsRepository _postsRepository;
        private readonly IAnnouncementRepository _announcementRepository;
        private readonly IMapper _mapper;
        private readonly IUriService _uriService;
        private readonly IEventParticipantsRepository _participationRepository;
        private readonly INotificationService _notificationService;
        private readonly IDomainToResponseMapper _domainToResponseMapper;

        public AnnouncementController(UserManager<AppUser> userManager, IPostsRepository postsRepository,
                                      IAnnouncementRepository announcementRepository, IMapper mapper,
                                      IUriService uriService, IEventParticipantsRepository participationRepository,
                                      INotificationService notificationService, IDomainToResponseMapper domainToResponseMapper, 
                                      IOptions<EventTypes> eventTypes)
        {
            this._userManager = userManager;
            this._postsRepository = postsRepository;
            this._announcementRepository = announcementRepository;
            this._mapper = mapper;
            this._uriService = uriService;
            this._participationRepository = participationRepository;
            this._notificationService = notificationService;
            _domainToResponseMapper = domainToResponseMapper;
            _eventTypes = eventTypes.Value;
        }




        /// <summary>
        /// This endpoint returns an announcement by Id.
        /// Can be viewed by an event participator / admin / host
        /// </summary>
        /// <param name="announcementId">The announcement Id</param>
        /// <response code="200">Success</response>
        /// <response code="400">Not Found</response>
        /// <response code="403">Requester is not participating in the event / not an admin or host either</response>
        [ProducesResponseType(typeof(Response<GetAnnouncement>),200)]
        [ProducesResponseType(typeof(SingleError),403)]
        [ProducesResponseType(404)]
        [HttpGet(ApiRoutes.Announcements.Get)]
        public async Task<IActionResult> GetAnnouncement([FromRoute] int announcementId)
        {
            var announcement = await _announcementRepository.GetByIdAsync(announcementId);
            if(announcement == null)
            {
                return NotFound();
            }
            var requester = await _userManager.FindByIdAsync(HttpContext.GetUserId());
            var isParticipator = await _participationRepository.IsParticipatorAsync(announcement.PostId, requester.Id);
            var isAdmin = await RoleCheckingHelper.CheckIfAdmin(_userManager, requester);
            var isOwner = await _participationRepository.IsPostOwnerAsync(announcement.PostId, requester.Id);

            if(!(isAdmin || isParticipator || isOwner))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "You do not participate in this event / You are not an Administrator" });
            }

            return Ok(new Response<GetAnnouncement>(_mapper.Map<GetAnnouncement>(announcement)));
        }


        /// <summary>
        /// This endpoint returns all announcements related to a post. The newest come on top of the list.
        /// Can be viewed by event participator / admin / host
        /// </summary>
        /// <param name="postId">The post Id</param>
        /// <response code="200">Success</response>
        /// <response code="204">No announcements for this post</response>
        /// <response code="404">Post with such id was not found</response>
        /// <response code="403">Requester is not participating in the event / not an admin or host either</response>
        [ProducesResponseType(typeof(Response<List<GetAnnouncement>>), 200)]
        [ProducesResponseType(typeof(SingleError), 403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(204)]
        [HttpGet(ApiRoutes.Announcements.GetAll)]
        public async Task<IActionResult> GetAllAnnouncements([FromRoute] int postId)
        {
            var requester = await _userManager.FindByIdAsync(HttpContext.GetUserId());
            var post = await _postsRepository.GetPlainPostAsync(postId);
            if (post == null)
            {
                return NotFound();
            }

            var isParticipator = await _participationRepository.IsParticipatorAsync(postId, requester.Id);
            var isAdmin = await RoleCheckingHelper.CheckIfAdmin(_userManager, requester);

            if (!(isAdmin || isParticipator))
            {
                if (!(await _participationRepository.IsPostOwnerAsync(postId, requester.Id)))
                    return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "You do not participate in this event / You are not an Administrator / Not owner of post" });
            }

            var announcements = await _announcementRepository.GetAllByPostIdAsync(postId);
            if(announcements.Count == 0)
            {
                return NoContent();
            }

            return Ok(new Response<List<GetAnnouncement>>(_mapper.Map<List<GetAnnouncement>>(announcements)));
        }


       

        /// <summary>
        /// This endoint is used for creating an announcement.
        /// Can be created only by event host / admin.
        /// All event attendees will get a push notification when the host creates an announcement.
        /// </summary>
        /// <param name="createAnnouncementRequest">The announcement data, the message should be not less than 10 characters</param>
        /// <response code="201">Successfuly created</response>
        /// <response code="400">Persistence error</response>
        /// <response code="403">Requester is not not an admin or host either</response>
        [ProducesResponseType(typeof(Response<CreateAnnouncementResponse>), 201)]
        [ProducesResponseType(typeof(SingleError), 400)]
        [ProducesResponseType(typeof(SingleError), 403)]
        [HttpPost(ApiRoutes.Announcements.Create)]
        public async Task<IActionResult> CreateAnnouncement([FromBody] CreateAnnouncementRequest createAnnouncementRequest)
        {
            var user = await _userManager.FindByIdAsync(HttpContext.GetUserId());
            if(user == null)
            {
                return BadRequest();
            }
            var isOwnerOrAdmin = await _postsRepository.IsPostOwnerOrAdminAsync(createAnnouncementRequest.PostId, user.Id);
            if (!isOwnerOrAdmin)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "You do not own this post / You are not an Administrator" });
            }

            var announcement = _mapper.Map<CreateAnnouncementRequest, Announcement>(createAnnouncementRequest);
            var result = await _announcementRepository.AddAsync(announcement);
            if (!result)
            {
                return BadRequest(new SingleError { Message = "This announcement could not be persisted!" });
            }

            // notify participants about new announcement
            var participants = await _postsRepository.GetParticipantsIdAsync(createAnnouncementRequest.PostId);
            if(participants.Count !=0)
            {
                var post = await _postsRepository.GetPlainPostAsync(createAnnouncementRequest.PostId);
                await _notificationService.NotifyParticipantsNewAnnouncementAsync(participants, post.Event.Title);
            }

            var locationUri = _uriService.GetAnnouncementUri(announcement.Id.ToString());
            return Created(locationUri, new Response<CreateAnnouncementResponse>(_mapper.Map<CreateAnnouncementResponse>(announcement)));
        }



        /// <summary>
        /// This endpoint is used for updating an announcement.
        /// Can be updated only by event host / admin
        /// </summary>
        /// <param name="announcementId">The announcement Id</param>
        /// <param name="updateRequest">Updated data, the message should be not less than 10 characters</param>
        /// <response code="200">Successfuly updated</response>
        /// <response code="400">Update could not be persisted</response>
        /// <response code="403">Requester is not the host / admin</response>
        /// <response code="404">Announcements does not exist</response>
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(SingleError), 400)]
        [ProducesResponseType(typeof(SingleError), 403)]
        [ProducesResponseType(typeof(SingleError), 404)]
        [HttpPut(ApiRoutes.Announcements.Update)]
        public async Task<IActionResult> UpdateAnnouncement([FromRoute] int announcementId, [FromBody] UpdateAnnouncementRequest updateRequest)
        {
            var requester = await _userManager.FindByIdAsync(HttpContext.GetUserId());
            var announcement = await _announcementRepository.GetByIdAsync(announcementId);
            if (announcement == null)
            {
                return NotFound(new SingleError { Message = "Announcement does not exist" });
            }

            var postId = announcement.PostId;
            var isOwnerOrAdmin = await _postsRepository.IsPostOwnerOrAdminAsync(postId, requester.Id);
            if (!(isOwnerOrAdmin))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "You do not own this post / You are not an Administrator" });
            }

            _mapper.Map(updateRequest, announcement);
            var result = await _announcementRepository.UpdateAsync(announcement);
            if (!result)
                return BadRequest(new SingleError { Message = "Update Failed" });

            return Ok();
        }



        /// <summary>
        /// This endpoint is used for deleting an announcement.
        /// Can be deleted only by event host / admin
        /// </summary>
        /// <param name="announcementId">The announcement id</param>
        /// <response code="204">Successfuly deleted</response>
        /// <response code="400">Delete failed</response>
        /// <response code="403">Requester is not the host / admin</response>
        /// <response code="404">Announcements does not exist</response>
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(SingleError), 400)]
        [ProducesResponseType(typeof(SingleError), 403)]
        [ProducesResponseType(typeof(SingleError), 404)]
        [HttpDelete(ApiRoutes.Announcements.Delete)]
        public async Task<IActionResult> DeleteAnnouncement([FromRoute] int announcementId)
        {
            var requester = await _userManager.FindByIdAsync(HttpContext.GetUserId());
            var announcement = await _announcementRepository.GetByIdAsync(announcementId);
            if (announcement == null)
            {
                return NotFound(new SingleError { Message = "Announcement does not exist" });
            }

            var postId = announcement.PostId;
            var isOwner = await _postsRepository.IsPostOwnerOrAdminAsync(postId, requester.Id);
            var isAdmin = await RoleCheckingHelper.CheckIfAdmin(_userManager, requester);
            if (!(isAdmin || isOwner))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "You do not own this post / You are not an Administrator" });
            }

            var result = await _announcementRepository.DeleteAsync(announcementId);
            if (!result)
                return BadRequest(new SingleError { Message = "Delete Failed" });

            return NoContent();
        }


        /// <summary>
        /// This endpoint returns miniposts of user's posts that have any announcements.
        /// The returned list of posts is sorted descending by the timestamp of their last announcement.
        /// I.e miniposts for the outbox.
        /// </summary>
        /// <returns></returns>
        [HttpGet(ApiRoutes.Announcements.GetOutbox)]
        public async Task<IActionResult> GetAllWithOutbox()
        {
            var user = await _userManager.FindByIdAsync(HttpContext.GetUserId());
            if (user == null)
                return BadRequest(new SingleError { Message = "The requester is not a registered user" });

            var result = await _announcementRepository.GetEventsWithOutbox(user.Id);

            if (result.Count == 0)
                return NoContent();

            var resultList = new List<MiniPostForAnnouncements>();
            foreach (var post in result)
            {
                var mappedPost = _domainToResponseMapper.MapMiniPostForAnnouncementsList(post, _eventTypes);
                resultList.Add(mappedPost);
            }

            return Ok(new Response<List<MiniPostForAnnouncements>> { Data = resultList });
        }


        /// <summary>
        /// This endpoint returns all events that the user will attend or has attended already, and that contains any announcements.
        /// The returned list of posts is sorted descending by the timestamp of their last announcement. 
        /// i.e the post with the most recent announcement comes on top of the list.
        /// </summary>
        /// <response code="200">Returns a custom mini post for announcement data.</response>
        /// <response code="204">No announcements to display</response>
        [ProducesResponseType(typeof(Response<List<MiniPostForAnnouncements>>), 200)]
        [ProducesResponseType(204)]
        [HttpGet(ApiRoutes.Announcements.GetInbox)]
        public async Task<IActionResult> GetAllWithAnnouncements()
        {
            var user = await _userManager.FindByIdAsync(HttpContext.GetUserId());
            if (user == null)
                return BadRequest(new SingleError { Message = "The requester is not a registered user" });

            var result = await _announcementRepository.GetAttendedEventsWithAnnouncements(user.Id);

            if (result.Count == 0)
                return NoContent();

            var resultList = new List<MiniPostForAnnouncements>();
            foreach (var post in result)
            {
                var mappedPost = _domainToResponseMapper.MapMiniPostForAnnouncementsList(post, _eventTypes);
                resultList.Add(mappedPost);
            }

            return Ok(new Response<List<MiniPostForAnnouncements>> { Data = resultList });
        }
    }
}
