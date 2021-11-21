using BingoAPI.Extensions;
using BingoAPI.Models;
using BingoAPI.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IOptions<OneSignalNotificationSettigs> _oneSignalSettings;
        private readonly IOptions<NotificationTemplates> _notificationTemplates;
        private readonly IHttpContextAccessor _httpContext;
        private readonly IErrorService _errorService;
        private readonly HttpRequestMessage _request;
        private readonly HttpClient _httpClient;

        public NotificationService(IOptions<OneSignalNotificationSettigs> oneSignalSettings, IOptions<NotificationTemplates> notificationTemplates,
                                   IHttpClientFactory clientFactory, IHttpContextAccessor httpContext, IErrorService errorService)
        {
            this._oneSignalSettings = oneSignalSettings;
            this._notificationTemplates = notificationTemplates;
            this._httpContext = httpContext;
            this._errorService = errorService;

            // request configuration
            _httpClient = clientFactory.CreateClient();
            _httpClient.DefaultRequestHeaders.Add("authorization", oneSignalSettings.Value.Authorization);
            _request = new HttpRequestMessage(HttpMethod.Post, oneSignalSettings.Value.EndPoint);           
        }


        public async Task NotifyAttendEventRequestAcceptedAsync(List<string> usersId, string eventTitle, int postId)
        {
            //var list = new List<string> { "d150bc2e-b5ac-4787-b4ed-20e421b24d9d" };
            var obj = new            
            {
                app_id = _oneSignalSettings.Value.AppId,
                contents = new
                {
                    en = string.Format(_notificationTemplates.Value.AttendEventRequestAccepted.en, eventTitle),
                    ru = string.Format(_notificationTemplates.Value.AttendEventRequestAccepted.ru, eventTitle)
                },
                data = new
                {
                    type = "event",
                    postId = postId
                },
                headings = new 
                {
                    en = _notificationTemplates.Value.Heading.en,
                    ru = _notificationTemplates.Value.Heading.ru
                },
                include_external_user_ids = usersId/*list*/
            };

            await SerializeNotificationAsync(obj);
        }



        public async Task NotifyHostNewParticipationRequestAsync(List<string> usersId, string fullname, int postId)
        {            
            var obj = new
            {
                app_id = _oneSignalSettings.Value.AppId,
                contents = new
                {
                    en = string.Format(_notificationTemplates.Value.HousePartyAttendRequest.en, fullname),
                    ru = string.Format(_notificationTemplates.Value.HousePartyAttendRequest.ru, fullname)
                },
                data = new
                {
                    type = "pendingRequests",
                    postId = postId
                },
                headings = new
                {
                    en = _notificationTemplates.Value.Heading.en,
                    ru = _notificationTemplates.Value.Heading.ru
                },
                include_external_user_ids = usersId
            };

            await SerializeNotificationAsync(obj);
        }


        public async Task NotifyParticipantsEventUpdatedAsync(List<string> usersId, string eventTitle, int postId)
        {           
            var obj = new
            {
                app_id = _oneSignalSettings.Value.AppId,
                contents = new
                {
                    en = string.Format(_notificationTemplates.Value.EventUpdated.en, eventTitle),
                    ru = string.Format(_notificationTemplates.Value.EventUpdated.ru, eventTitle)
                },
                data = new
                {
                    type = "event",
                    postId = postId.ToString()
                },
                headings = new
                {
                    en = _notificationTemplates.Value.Heading.en,
                    ru = _notificationTemplates.Value.Heading.ru
                },
                include_external_user_ids = usersId
            };
            await SerializeNotificationAsync(obj);
        }

        public async Task NotifyParticipantsEventDeletedAsync(List<string> usersId, string eventTitle)
        {            
            var obj = new
            {
                app_id = _oneSignalSettings.Value.AppId,
                contents = new
                {
                    en = string.Format(_notificationTemplates.Value.EventDeleted.en, eventTitle),
                    ru = string.Format(_notificationTemplates.Value.EventDeleted.ru, eventTitle)
                },
                data = new
                {
                    type = "deletedEvent",
                    title = eventTitle
                },
                headings = new
                {
                    en = _notificationTemplates.Value.Heading.en,
                    ru = _notificationTemplates.Value.Heading.ru
                    
                },
                include_external_user_ids = usersId
            };
            await SerializeNotificationAsync(obj);
        }

        public async Task NotifyParticipantsNewAnnouncementAsync(List<string> usersId, string eventTitle, int postId)
        {
            var obj = new
            {
                app_id = _oneSignalSettings.Value.AppId,
                contents = new
                {
                    en = string.Format(_notificationTemplates.Value.NewAnnouncement.en, eventTitle),
                    ru = string.Format(_notificationTemplates.Value.NewAnnouncement.ru, eventTitle)
                    
                },
                data = new
                {
                    type = "announcements",
                    postId = postId
                },
                headings = new
                {
                    en = _notificationTemplates.Value.Heading.en,
                    ru = _notificationTemplates.Value.Heading.ru
                },
                include_external_user_ids = usersId
            };
            await SerializeNotificationAsync(obj);
        }

        private async Task SerializeNotificationAsync(Object obj)
        {            
            var param = JsonConvert.SerializeObject(obj);
            var data = new StringContent(param, Encoding.UTF8, "application/json");
            await SendNotificationAsync(data);                                                
        }

                
        private async Task SendNotificationAsync(StringContent buffer)
        {            
            var response = await _httpClient.PostAsync(_request.RequestUri, buffer);

            if (!response.IsSuccessStatusCode)
            {
                // logg error
                var errorObj = new ErrorLog
                {
                    Date = DateTime.UtcNow,
                    ExtraData = "Sending notification failed...",
                    UserId = _httpContext.HttpContext.GetUserId(),
                    Message = await response.Content.ReadAsStringAsync()
                };
                var ok = await _errorService.AddErrorAsync(errorObj);               
            }
        }
                
    }
}
