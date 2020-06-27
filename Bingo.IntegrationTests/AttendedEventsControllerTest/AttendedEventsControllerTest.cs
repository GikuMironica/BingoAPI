using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.EventAttendee;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.Post;
using Bingo.IntegrationTests.AnnouncementControllerTest;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Priority;

namespace Bingo.IntegrationTests.AttendedEventsControllerTest
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class AttendedEventsControllerTest : AttendEventsIntegrationTest
    {

// ATTEND EVENT TEST

        [Fact, Priority(1)]
        public async Task Attend_Valid_StreetParty_Event()
        {
            // Arrange
            var host = await AuthenticateAsync();
            var post = await CreateSamplePostAsync();
            var guest = await AuthenticateAsync();

            // Act
            var attendReq = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post.Id.ToString()), null);

            // Assert
            attendReq.StatusCode.Should().Be(HttpStatusCode.OK);

        }


        [Fact, Priority(1)]
        public async Task Attend_HouseParty_Event()
        {
            // Arrange
            var host = await AuthenticateAsync();
            var post = await CreateSampleHousePartyAsync(5);
            var guest = await AuthenticateAsync();

            // Act
            var attendReq = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post.Id.ToString()), null);
            var getPostReqBefore = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", post.Id.ToString()));
            var postDataBefore = await getPostReqBefore.Content.ReadFromJsonAsync<Response<PostResponse>>();
            
            UpdateToken(host.JWT);

            var acceptUser = new AttendeeRequest
            {
                PostId = post.Id,
                AttendeeId = guest.UserId
            };
            var acceptReq = await TestClient.PostAsJsonAsync(ApiRoutes.EventAttendees.Accept, acceptUser);
            var getPostReqAfter = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", post.Id.ToString()));
            var postDataAfter = await getPostReqAfter.Content.ReadFromJsonAsync<Response<PostResponse>>();

            // Assert
            attendReq.StatusCode.Should().Be(HttpStatusCode.OK);
            acceptReq.StatusCode.Should().Be(HttpStatusCode.OK);

            Assert.NotNull(postDataBefore.Data);
            Assert.NotNull(postDataAfter.Data);

            Assert.Equal(5, postDataBefore.Data.AvailableSlots);
            Assert.Equal(4, postDataAfter.Data.AvailableSlots);
        }
    }
}
