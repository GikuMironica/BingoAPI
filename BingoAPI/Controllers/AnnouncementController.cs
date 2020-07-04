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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BingoAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Admin,User")]
    [Produces("application/json")]
    public class AnnouncementController : Controller
    {
        private readonly UserManager<AppUser> userManager;
        private readonly IPostsRepository postsRepository;
        private readonly IAnnouncementRepository announcementRepository;
        private readonly IMapper mapper;
        private readonly IUriService uriService;
        private readonly IEventParticipantsRepository participationRepository;
        private readonly INotificationService notificationService;

        public AnnouncementController(UserManager<AppUser> userManager, IPostsRepository postsRepository,
                                      IAnnouncementRepository announcementRepository, IMapper mapper,
                                      IUriService uriService, IEventParticipantsRepository participationRepository,
                                      INotificationService notificationService)
        {
            this.userManager = userManager;
            this.postsRepository = postsRepository;
            this.announcementRepository = announcementRepository;
            this.mapper = mapper;
            this.uriService = uriService;
            this.participationRepository = participationRepository;
            this.notificationService = notificationService;
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
            var announcement = await announcementRepository.GetByIdAsync(announcementId);
            if(announcement == null)
            {
                return NotFound();
            }
            var requester = await userManager.FindByIdAsync(HttpContext.GetUserId());
            var isParticipator = await participationRepository.IsParticipatorAsync(announcement.PostId, requester.Id);
            var isAdmin = await RoleCheckingHelper.CheckIfAdmin(userManager, requester);
            var isOwner = await participationRepository.IsPostOwnerAsync(announcement.PostId, requester.Id);

            if(!(isAdmin || isParticipator || isOwner))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "You do not participate in this event / You are not an Administrator" });
            }

            return Ok(new Response<GetAnnouncement>(mapper.Map<GetAnnouncement>(announcement)));
        }


        /// <summary>
        /// This endpoint returns all announcements related to a post.
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
            var requester = await userManager.FindByIdAsync(HttpContext.GetUserId());
            var post = await postsRepository.GetPlainPostAsync(postId);
            if (post == null)
            {
                return NotFound();
            }

            var isParticipator = await participationRepository.IsParticipatorAsync(postId, requester.Id);
            var isAdmin = await RoleCheckingHelper.CheckIfAdmin(userManager, requester);

            if (!(isAdmin || isParticipator))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "You do not participate in this event / You are not an Administrator / Not owner of post" });
            }

            var announcements = await announcementRepository.GetAllByPostIdAsync(postId);
            if(announcements.Count == 0)
            {
                return NoContent();
            }

            return Ok(new Response<List<GetAnnouncement>>(mapper.Map<List<GetAnnouncement>>(announcements)));
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
            var User = await userManager.FindByIdAsync(HttpContext.GetUserId());
            if(User == null)
            {
                return BadRequest();
            }
            var isOwnerOrAdmin = await postsRepository.IsPostOwnerOrAdminAsync(createAnnouncementRequest.PostId, User.Id);
            if (!isOwnerOrAdmin)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "You do not own this post / You are not an Administrator" });
            }

            var announcement = mapper.Map<CreateAnnouncementRequest, Announcement>(createAnnouncementRequest);
            var result = await announcementRepository.AddAsync(announcement);
            if (!result)
            {
                return BadRequest(new SingleError { Message = "This announcement could not be persisted!" });
            }

            // notify participants about new announcement
            var participants = await postsRepository.GetParticipantsIdAsync(createAnnouncementRequest.PostId);
            var post = await postsRepository.GetPlainPostAsync(createAnnouncementRequest.PostId);
            await notificationService.NotifyParticipantsNewAnnouncementAsync(participants, post.Event.Title);

            var locationUri = uriService.GetAnnouncementUri(announcement.Id.ToString());
            return Created(locationUri, new Response<CreateAnnouncementResponse>(mapper.Map<CreateAnnouncementResponse>(announcement)));
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
            var requester = await userManager.FindByIdAsync(HttpContext.GetUserId());
            var announcement = await announcementRepository.GetByIdAsync(announcementId);
            if (announcement == null)
            {
                return NotFound(new SingleError { Message = "Announcement does not exist" });
            }

            var postId = announcement.PostId;
            var isOwnerOrAdmin = await postsRepository.IsPostOwnerOrAdminAsync(postId, requester.Id);
            if (!(isOwnerOrAdmin))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "You do not own this post / You are not an Administrator" });
            }

            mapper.Map<UpdateAnnouncementRequest, Announcement>(updateRequest, announcement);
            var result = await announcementRepository.UpdateAsync(announcement);
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
            var requester = await userManager.FindByIdAsync(HttpContext.GetUserId());
            var announcement = await announcementRepository.GetByIdAsync(announcementId);
            if (announcement == null)
            {
                return NotFound(new SingleError { Message = "Announcement does not exist" });
            }

            var postId = announcement.PostId;
            var isOwner = await postsRepository.IsPostOwnerOrAdminAsync(postId, requester.Id);
            var isAdmin = await RoleCheckingHelper.CheckIfAdmin(userManager, requester);
            if (!(isAdmin || isOwner))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "You do not own this post / You are not an Administrator" });
            }

            var result = await announcementRepository.DeleteAsync(announcementId);
            if (!result)
                return BadRequest(new SingleError { Message = "Delete Failed" });

            return NoContent();
        }
    }
}
