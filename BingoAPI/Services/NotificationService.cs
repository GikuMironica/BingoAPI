using BingoAPI.Extensions;
using BingoAPI.Models;
using BingoAPI.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IOptions<OneSignalNotificationSettigs> oneSignalSettings;
        private readonly IOptions<NotificationTemplates> notificationTemplates;
        private readonly IHttpContextAccessor httpContext;
        private readonly IErrorService errorService;
        private readonly HttpRequestMessage request;
        private readonly HttpClient httpClient;

        public NotificationService(IOptions<OneSignalNotificationSettigs> oneSignalSettings, IOptions<NotificationTemplates> notificationTemplates,
                                   IHttpClientFactory clientFactory, IHttpContextAccessor httpContext, IErrorService errorService)
        {
            this.oneSignalSettings = oneSignalSettings;
            this.notificationTemplates = notificationTemplates;
            this.httpContext = httpContext;
            this.errorService = errorService;

            // request configuration
            httpClient = clientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("authorization", oneSignalSettings.Value.Authorization);
            request = new HttpRequestMessage(HttpMethod.Post, oneSignalSettings.Value.EndPoint);           
        }



        public async Task NotifyAttendEventRequestAcceptedAsync(List<string> usersId, string eventTitle)
        {
            //var list = new List<string> { "d150bc2e-b5ac-4787-b4ed-20e421b24d9d" };
            var obj = new            
            {
                app_id = oneSignalSettings.Value.AppId,
                contents = new
                {
                    en = string.Format(notificationTemplates.Value.AttendEventRequestAccepted.en, eventTitle),
                    ru = string.Format(notificationTemplates.Value.AttendEventRequestAccepted.ru, eventTitle)
                },
                headings = new 
                {
                    en = notificationTemplates.Value.Heading.en,
                    ru = notificationTemplates.Value.Heading.ru
                },
                include_external_user_ids = usersId/*list*/
            };

            await SerializeNotificationAsync(obj);
        }



        public async Task NotifyHostNewParticipationRequestAsync(List<string> usersId, string fullname)
        {            
            var obj = new
            {
                app_id = oneSignalSettings.Value.AppId,
                contents = new
                {
                    en = string.Format(notificationTemplates.Value.HousePartyAttendRequest.en, fullname),
                    ru = string.Format(notificationTemplates.Value.HousePartyAttendRequest.ru, fullname)
                },
                headings = new
                {
                    en = notificationTemplates.Value.Heading.en,
                    ru = notificationTemplates.Value.Heading.ru
                },
                include_external_user_ids = usersId
            };

            await SerializeNotificationAsync(obj);
        }


        public async Task NotifyParticipantsEventUpdatedAsync(List<string> usersId, string eventTitle)
        {           
            var obj = new
            {
                app_id = oneSignalSettings.Value.AppId,
                contents = new
                {
                    en = string.Format(notificationTemplates.Value.EventUpdated.en, eventTitle),
                    ru = string.Format(notificationTemplates.Value.EventUpdated.ru, eventTitle)
                },
                headings = new
                {
                    en = notificationTemplates.Value.Heading.en,
                    ru = notificationTemplates.Value.Heading.ru
                },
                include_external_user_ids = usersId
            };
            await SerializeNotificationAsync(obj);
        }

        public async Task NotifyParticipantsEventDeletedAsync(List<string> usersId, string eventTitle)
        {            
            var obj = new
            {
                app_id = oneSignalSettings.Value.AppId,
                contents = new
                {
                    en = string.Format(notificationTemplates.Value.EventDeleted.en, eventTitle),
                    ru = string.Format(notificationTemplates.Value.EventDeleted.ru, eventTitle)
                },
                headings = new
                {
                    en = notificationTemplates.Value.Heading.en,
                    ru = notificationTemplates.Value.Heading.ru
                },
                include_external_user_ids = usersId
            };
            await SerializeNotificationAsync(obj);
        }

        public async Task NotifyParticipantsNewAnnouncementAsync(List<string> usersId, string eventTitle)
        {
            var obj = new
            {
                app_id = oneSignalSettings.Value.AppId,
                contents = new
                {
                    en = string.Format(notificationTemplates.Value.NewAnnouncement.en, eventTitle),
                    ru = string.Format(notificationTemplates.Value.NewAnnouncement.ru, eventTitle)
                },
                headings = new
                {
                    en = notificationTemplates.Value.Heading.en,
                    ru = notificationTemplates.Value.Heading.ru
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
            var response = await httpClient.PostAsync(request.RequestUri, buffer);

            if (!response.IsSuccessStatusCode)
            {
                // logg error
                var errorObj = new ErrorLog
                {
                    Date = DateTime.UtcNow,
                    ExtraData = "Sending notificatification failed...",
                    UserId = httpContext.HttpContext.GetUserId(),
                    Message = await response.Content.ReadAsStringAsync()
                };
                var ok = await errorService.AddErrorAsync(errorObj);               
            }
        }
                
    }
}
