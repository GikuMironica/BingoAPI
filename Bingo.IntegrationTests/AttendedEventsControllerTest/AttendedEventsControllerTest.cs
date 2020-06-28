using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.EventAttendee;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.AttendedEvent;
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


        [Fact, Priority(1)]
        public async Task Attend_HouseParty_Twice_Event()
        {
            // Arrange
            var host = await AuthenticateAsync();
            var post = await CreateSampleHousePartyAsync(5);
            var guest = await AuthenticateAsync();

            // Act
            var attendReq = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post.Id.ToString()), null);
            var attendReq2 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post.Id.ToString()), null);
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
            attendReq2.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            acceptReq.StatusCode.Should().Be(HttpStatusCode.OK);

            Assert.NotNull(postDataBefore.Data);
            Assert.NotNull(postDataAfter.Data);

            Assert.Equal(5, postDataBefore.Data.AvailableSlots);
            Assert.Equal(4, postDataAfter.Data.AvailableSlots);
        }


        [Fact, Priority(1)]
        public async Task Attend_Unexisting_Event()
        {
            // Arrange
            var guest = await AuthenticateAsync();

            // Act
            var attendReq = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", "923400099"), null);
            
            // Assert
            attendReq.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }


        [Fact, Priority(1)]
        public async Task Attend_HouseParty_InPast()
        {
            var stime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 1700;
            var etime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 300;

            // Arrange
            var host = await AuthenticateAsync();
            var post = await CreateSampleHousePartyAsync(5, stime, etime);
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
            var getPostReqAfter = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", post.Id.ToString()));
            var postDataAfter = await getPostReqAfter.Content.ReadFromJsonAsync<Response<PostResponse>>();

            // Assert
            attendReq.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            Assert.NotNull(postDataBefore.Data);
            Assert.NotNull(postDataAfter.Data);

            Assert.Equal(5, postDataBefore.Data.AvailableSlots);
            Assert.Equal(5, postDataAfter.Data.AvailableSlots);
        }


        [Fact, Priority(1)]
        public async Task Unattend_Event_After_Accepted()
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

            UpdateToken(guest.JWT);
            var unattendReq = await TestClient.PostAsync(ApiRoutes.AttendedEvents.UnAttend.Replace("{postId}", post.Id.ToString()), null);
            var getPostReqAfterUnttend = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", post.Id.ToString()));
            var postDataAfterUnattend = await getPostReqAfterUnttend.Content.ReadFromJsonAsync<Response<PostResponse>>();

            // Assert
            attendReq.StatusCode.Should().Be(HttpStatusCode.OK);
            acceptReq.StatusCode.Should().Be(HttpStatusCode.OK);
            unattendReq.StatusCode.Should().Be(HttpStatusCode.OK);

            Assert.NotNull(postDataBefore.Data);
            Assert.NotNull(postDataAfter.Data);
            Assert.NotNull(postDataAfterUnattend.Data);

            Assert.Equal(5, postDataBefore.Data.AvailableSlots);
            Assert.Equal(4, postDataAfter.Data.AvailableSlots);
            Assert.Equal(5, postDataAfterUnattend.Data.AvailableSlots);
        }


        [Fact, Priority(1)]
        public async Task Unattend_Event_Before_Accepted()
        {
            // Arrange
            var host = await AuthenticateAsync();
            var post = await CreateSampleHousePartyAsync(5);
            var guest = await AuthenticateAsync();

            // Act
            var attendReq = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post.Id.ToString()), null);
            var getPostReqBefore = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", post.Id.ToString()));
            var postDataBefore = await getPostReqBefore.Content.ReadFromJsonAsync<Response<PostResponse>>();

            UpdateToken(guest.JWT);
            var unattendReq = await TestClient.PostAsync(ApiRoutes.AttendedEvents.UnAttend.Replace("{postId}", post.Id.ToString()), null);
            var getPostReqAfterUnttend = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", post.Id.ToString()));
            var postDataAfterUnattend = await getPostReqAfterUnttend.Content.ReadFromJsonAsync<Response<PostResponse>>();

            // Assert
            attendReq.StatusCode.Should().Be(HttpStatusCode.OK);
            unattendReq.StatusCode.Should().Be(HttpStatusCode.OK);

            Assert.NotNull(postDataBefore.Data);
            Assert.NotNull(postDataAfterUnattend.Data);

            Assert.Equal(5, postDataBefore.Data.AvailableSlots);
            Assert.Equal(5, postDataAfterUnattend.Data.AvailableSlots);
        }


        [Fact, Priority(1)]
        public async Task Unattend_Unexisting_Event()
        {
            // Arrange
            var guest = await AuthenticateAsync();

            // Act
            var attendReq = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", "98989987"), null);

            // Assert
            attendReq.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }


        [Fact, Priority(1)]
        public async Task Get_All_Active_Attended_Events()
        {
            // Arrange
            var guest = await AuthenticateAsync();

            var host1 = await AuthenticateAsync();
            var post1 = await CreateSamplePostAsync();


            var host2 = await AuthenticateAsync();
            var post2 = await CreateSamplePostAsync();


            var host3 = await AuthenticateAsync();
            var post3 = await CreateSamplePostAsync();


            // Act
            var attendReq1 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post1.Id.ToString()), null);
            var attendReq2 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post2.Id.ToString()), null);
            var attendReq3 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post3.Id.ToString()), null);

            var getPosts = await TestClient.GetAsync(ApiRoutes.AttendedEvents.GetActiveAttendedPosts);
            var postData = await getPosts.Content.ReadFromJsonAsync<Response<List<ActiveAttendedEvent>>>();


            // Assert
            attendReq1.StatusCode.Should().Be(HttpStatusCode.OK);
            attendReq2.StatusCode.Should().Be(HttpStatusCode.OK);
            attendReq3.StatusCode.Should().Be(HttpStatusCode.OK);

            Assert.Equal(3, postData.Data.Count);
        }


        [Fact, Priority(1)]
        public async Task Get_All_Active_Attended_HouseParty_When_Not_Accepted()
        {
            // Arrange
            var guest = await AuthenticateAsync();

            var host1 = await AuthenticateAsync();
            var post1 = await CreateSampleHousePartyAsync(7);


            var host2 = await AuthenticateAsync();
            var post2 = await CreateSampleHousePartyAsync(8);


            var host3 = await AuthenticateAsync();
            var post3 = await CreateSampleHousePartyAsync(9);


            // Act
            var attendReq1 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post1.Id.ToString()), null);
            var attendReq2 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post2.Id.ToString()), null);
            var attendReq3 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post3.Id.ToString()), null);

            var getPosts = await TestClient.GetAsync(ApiRoutes.AttendedEvents.GetActiveAttendedPosts);
            var postData = await getPosts.Content.ReadFromJsonAsync<Response<string>>();


            // Assert
            attendReq1.StatusCode.Should().Be(HttpStatusCode.OK);
            attendReq2.StatusCode.Should().Be(HttpStatusCode.OK);
            attendReq3.StatusCode.Should().Be(HttpStatusCode.OK);

            Assert.Equal("No events attended", postData.Data);
        }
    }
}
