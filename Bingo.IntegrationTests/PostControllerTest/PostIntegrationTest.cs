using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.Post;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.Post;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Bingo.IntegrationTests.PostControllerTest
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
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new NullReferenceException();
            }
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<Response<CreatePostResponse>>();
        }

        protected async Task<bool> UpdatePostAsync(UpdatePostRequest updatePostRequest, int id)
        {
            string tag1 = null;
            string tag2 = null;
            if (updatePostRequest.TagNames != null)
            {
                tag1 = updatePostRequest.TagNames.FirstOrDefault();
                if(updatePostRequest.TagNames.Count >1)
                    tag2 = updatePostRequest.TagNames.LastOrDefault();
            }

            List<KeyValuePair<string, string>> postFieldsCollection = new List<KeyValuePair<string, string>>
            {                                              
                new KeyValuePair<string, string>("TagNames", tag1),
                new KeyValuePair<string, string>("TagNames", tag2)
            };

            if (updatePostRequest.EventTime != null || updatePostRequest.EventTime != 0)
                postFieldsCollection.Add(new KeyValuePair<string, string>("EventTime", updatePostRequest.EventTime.ToString()));
            if (updatePostRequest.EndTime != null || updatePostRequest.EndTime != 0)
                postFieldsCollection.Add(new KeyValuePair<string, string>("EndTime", updatePostRequest.EndTime.ToString()));
            if (updatePostRequest.Event != null)
            {
                if (updatePostRequest.Event.Slots != null)
                    postFieldsCollection.Add(new KeyValuePair<string, string>("Event.Slots", updatePostRequest.Event.Slots.ToString()));
                postFieldsCollection.Add(new KeyValuePair<string, string>("Event.Description", updatePostRequest.Event.Description));
                postFieldsCollection.Add(new KeyValuePair<string, string>("Event.Requirements", updatePostRequest.Event.Requirements));
                postFieldsCollection.Add(new KeyValuePair<string, string>("Event.Title", updatePostRequest.Event.Title));
                postFieldsCollection.Add(new KeyValuePair<string, string>("Event.EntrancePrice", updatePostRequest.Event.EntrancePrice.ToString()));
            }
               

            if (updatePostRequest.UserLocation != null)
            {
                if (updatePostRequest.UserLocation.Longitude != null)
                    postFieldsCollection.Add(new KeyValuePair<string, string>("UserLocation.Longitude", updatePostRequest.UserLocation.Longitude.ToString()));
                if(updatePostRequest.UserLocation.Latitude != null)
                    postFieldsCollection.Add(new KeyValuePair<string, string>("UserLocation.Latitude", updatePostRequest.UserLocation.Latitude.ToString()));

                postFieldsCollection.Add(new KeyValuePair<string, string>("UserLocation.Address", updatePostRequest.UserLocation.Address));
                postFieldsCollection.Add(new KeyValuePair<string, string>("UserLocation.City", updatePostRequest.UserLocation.City));
                postFieldsCollection.Add(new KeyValuePair<string, string>("UserLocation.Region", updatePostRequest.UserLocation.Region));
                postFieldsCollection.Add(new KeyValuePair<string, string>("UserLocation.EntityName", updatePostRequest.UserLocation.EntityName));
                postFieldsCollection.Add(new KeyValuePair<string, string>("UserLocation.Country", updatePostRequest.UserLocation.Country));
            };

            // should call get all posts, exclude all except house parties.
            var response = await TestClient.PostAsync(ApiRoutes.Posts.Update.Replace("{postId}", id.ToString()), new FormUrlEncodedContent(postFieldsCollection));
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new NullReferenceException();
            }
            return response.IsSuccessStatusCode;
        }
    }
}
