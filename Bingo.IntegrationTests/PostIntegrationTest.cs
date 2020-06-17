using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.Post;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.Post;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Bingo.IntegrationTests
{
    public class PostIntegrationTest : IntegrationTest
    {
        protected async Task<Response<CreatePostResponse>> CreatePostAsync(CreatePostRequest createPostRequest)
        {
            string tag1 = null;
            string tag2 = null;
            if(createPostRequest.Tags != null)
            {
                tag1 = createPostRequest.Tags.FirstOrDefault();
                tag2 = createPostRequest.Tags.LastOrDefault();
            }

            List<KeyValuePair<string, string>> postFieldsCollection = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Event.EventType",createPostRequest.Event.EventType.ToString()),
                new KeyValuePair<string, string>("Event.Slots",createPostRequest.Event.Slots.ToString()),
                new KeyValuePair<string, string>("Event.Description", createPostRequest.Event.Description),
                new KeyValuePair<string, string>("Event.Requirements", createPostRequest.Event.Requirements),
                new KeyValuePair<string, string>("Event.Title",createPostRequest.Event.Title),
                new KeyValuePair<string, string>("Event.EntrancePrice",createPostRequest.Event.EntrancePrice.ToString()),

                new KeyValuePair<string, string>("UserLocation.Longitude", createPostRequest.UserLocation.Longitude.ToString()),
                new KeyValuePair<string, string>("UserLocation.Latitude", createPostRequest.UserLocation.Latitude.ToString()),
                new KeyValuePair<string, string>("UserLocation.Address", createPostRequest.UserLocation.Address),
                new KeyValuePair<string, string>("UserLocation.City", createPostRequest.UserLocation.City),
                new KeyValuePair<string, string>("UserLocation.Region", createPostRequest.UserLocation.Region),
                new KeyValuePair<string, string>("UserLocation.EntityName", createPostRequest.UserLocation.EntityName),
                new KeyValuePair<string, string>("UserLocation.Country", createPostRequest.UserLocation.Country),

                new KeyValuePair<string, string>("EventTime", createPostRequest.EventTime.ToString()),
                new KeyValuePair<string, string>("EndTime", createPostRequest.EndTime.ToString()),

                new KeyValuePair<string, string>("Tags", tag1),
                new KeyValuePair<string, string>("Tags", tag2)
            };

            var response = await TestClient.PostAsync(ApiRoutes.Posts.Create, new FormUrlEncodedContent(postFieldsCollection));
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<Response<CreatePostResponse>>();
        }
    }
}
