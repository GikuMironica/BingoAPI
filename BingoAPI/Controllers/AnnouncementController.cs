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



        [HttpGet(ApiRoutes.Announcements.Get)]
        public async Task<IActionResult> GetAnnouncement([FromRoute] int announcementId)
        {
            var announcement = await announcementRepository.GetByIdAsync(announcementId);
            var requester = await userManager.FindByIdAsync(HttpContext.GetUserId());
            var isParticipator = await participationRepository.IsParticipatorAsync(announcement.PostId, requester.Id);
            var isAdmin = await RoleCheckingHelper.CheckIfAdmin(userManager, requester);

            if(!(isAdmin || isParticipator))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "You do not participate in this event / You are not an Administrator" });
            }

            return Ok(new Response<GetAnnouncement>(mapper.Map<GetAnnouncement>(announcement)));
        }

        [HttpGet(ApiRoutes.Announcements.GetAll)]
        public async Task<IActionResult> GetAllAnnouncements([FromRoute] int postId)
        {
            var requester = await userManager.FindByIdAsync(HttpContext.GetUserId());
            var isParticipator = await participationRepository.IsParticipatorAsync(postId, requester.Id);
            var isAdmin = await RoleCheckingHelper.CheckIfAdmin(userManager, requester);

            if (!(isAdmin || isParticipator))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "You do not participate in this event / You are not an Administrator" });
            }

            var announcements = await announcementRepository.GetAllByPostIdAsync(postId);
            return Ok(new Response<List<GetAnnouncement>>(mapper.Map<List<GetAnnouncement>>(announcements)));
        }

        [HttpPost(ApiRoutes.Announcements.Create)]
        public async Task<IActionResult> CreateAnnouncement([FromForm] CreateAnnouncementRequest createAnnouncementRequest)
        {
            var User = await userManager.FindByIdAsync(HttpContext.GetUserId());
            var isOwnerOrAdmin = await postsRepository.IsPostOwnerOrAdminAsync(createAnnouncementRequest.PostId, User.Id);
            if (!isOwnerOrAdmin)
            {
                return BadRequest(new SingleError { Message = "You do not own this post / Not an admin" });
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

        [HttpPut(ApiRoutes.Announcements.Update)]
        public async Task<IActionResult> UpdateAnnouncement([FromRoute] int announcementId, [FromBody] UpdateAnnouncementRequest updateRequest)
        {
            var requester = await userManager.FindByIdAsync(HttpContext.GetUserId());
            var announcement = await announcementRepository.GetByIdAsync(announcementId);
            if (announcement == null)
            {
                return BadRequest(new SingleError { Message = "Announcement does not exist" });
            }

            var postId = announcement.PostId;
            var isOwner = await postsRepository.IsPostOwnerOrAdminAsync(postId, requester.Id);
            var isAdmin = await RoleCheckingHelper.CheckIfAdmin(userManager, requester);
            if (!(isAdmin || isOwner))
            {
                return StatusCode(StatusCodes.Status403Forbidden, new SingleError { Message = "You do not own this post / You are not an Administrator" });
            }

            mapper.Map<UpdateAnnouncementRequest, Announcement>(updateRequest, announcement);
            var result = await announcementRepository.UpdateAsync(announcement);
            if (!result)
                return BadRequest(new SingleError { Message = "Update Failed" });

            return Ok();
        }

        [HttpDelete(ApiRoutes.Announcements.Delete)]
        public async Task<IActionResult> DeleteAnnouncement([FromRoute] int announcementId)
        {
            var requester = await userManager.FindByIdAsync(HttpContext.GetUserId());
            var announcement = await announcementRepository.GetByIdAsync(announcementId);
            if (announcement == null)
            {
                return BadRequest(new SingleError { Message = "Announcement does not exist" });
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
