using BingoAPI.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BingoAPI.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IOptions<OneSignalNotificationSettigs> oneSignalSettings;
        private readonly IOptions<NotificationTemplates> notificationTemplates;
        private readonly HttpWebRequest request;

        public NotificationService(IOptions<OneSignalNotificationSettigs> oneSignalSettings, IOptions<NotificationTemplates> notificationTemplates)
        {
            this.oneSignalSettings = oneSignalSettings;
            this.notificationTemplates = notificationTemplates;

            // request configuration
            request = WebRequest.Create(oneSignalSettings.Value.EndPoint) as HttpWebRequest;
            request.Headers.Add("authorization", oneSignalSettings.Value.Authorization);
            request.KeepAlive = true;
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";
                        
        }



        public async Task NotifyAttendEventRequestAcceptedAsync(List<string> usersId, string eventTitle)
        {
            var list = new List<string> { "d150bc2e-b5ac-4787-b4ed-20e421b24d9d" };
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
                include_external_user_ids = /*usersId*/list
            };

            await SerializeNotificationAsync(obj);
        }



        private async Task SerializeNotificationAsync(Object obj)
        {            
            var param = JsonConvert.SerializeObject(obj);
            byte[] byteArray = Encoding.UTF8.GetBytes(param);

            await SendNotificationAsync(byteArray);                                                
        }

                
        private async Task SendNotificationAsync(byte[] buffer)
        {
            await Task.Run(() => SendMessage(buffer));
        }


        private string SendMessage(byte[] buffer)
        {
            string responseContent = null;
            try
            {
                var writer = request.GetRequestStream();
                writer.Write(buffer, 0, buffer.Length);

                var response = request.GetResponse() as HttpWebResponse;
                var reader = new StreamReader(response.GetResponseStream());
                responseContent = reader.ReadToEnd();
                return responseContent;       
            }
            catch (WebException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
                // logg
                return null;
            }
        }

        
    }
}
