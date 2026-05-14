using Bingo.Contracts.V1.Requests.Post;
using Bingo.Contracts.V1.Responses.Post;
using Bingo.IntegrationTests.AnnouncementControllerTest;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Bingo.IntegrationTests.AttendedEventsControllerTest
{
    public class AttendEventsIntegrationTest : AnnouncementIntegrationTest
    {
        public async Task<Posts> CreateSampleHousePartyAsync(int? slots, long? starttime = null, long? endtime = null)
        {
            var createdPost = new CreatePostRequest
            {
                EventTime = starttime ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 10000,
                EndTime = endtime ?? DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 12000,
                UserLocation = new UserCompleteLocation
                {
                    Latitude = 48.3996,
                    Longitude = 9.22235,
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
                    EventType = 1,
                    Slots = slots
                },
                Tags = new List<string> { Guid.NewGuid().ToString(), "HouseParty" }
            };

            var result = await CreatePostAsync(createdPost);
            return result.Data;
        }
    }
}
