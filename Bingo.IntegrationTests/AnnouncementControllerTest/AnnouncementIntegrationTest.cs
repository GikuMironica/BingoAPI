using Bingo.Contracts.V1.Requests.Post;
using Bingo.Contracts.V1.Responses.Post;
using Bingo.IntegrationTests.PostControllerTest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bingo.IntegrationTests.AnnouncementControllerTest
{
    public class AnnouncementIntegrationTest : PostIntegrationTest
    {
        public async Task<Posts> CreateSamplePostAsync()
        {
            var createdPost = new CreatePostRequest
            {
                EventTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 10000,
                EndTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 12000,
                UserLocation = new UserCompleteLocation
                {
                    Latitude = 48.2996,
                    Longitude = 9.12235,
                    Address = "Street",
                    City = "UlmTest",
                    Country = "mars",
                    Region = "BW"
                },
                Event = new ContainedEvent
                {
                    Title = "Test Event for announcement",
                    Description = "Test post for announcement",
                    Requirements = "None",
                    EventType = 7
                },
                Tags = new List<string> { Guid.NewGuid().ToString(), "StreetParty" }
            };

            var result = await CreatePostAsync(createdPost);
            return result.Data;
        }
    }
}
