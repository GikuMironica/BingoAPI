using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.EventAttendee;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.EventAttendee;
using Bingo.Contracts.V1.Responses.Post;
using Bingo.IntegrationTests.AttendedEventsControllerTest;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Priority;

namespace Bingo.IntegrationTests.EventAttendeesControllerTest
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class EventAttendeesControllerTest : AttendEventsIntegrationTest
    {

// ACCEPT ATTENDEE TEST -----------------------------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact, Priority(1)]
        public async Task Accept_Attendee_HouseParty()
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
        public async Task Accept_Wrong_Attendee_HouseParty()
        {
            // Arrange
            var host = await AuthenticateAsync();
            var post = await CreateSampleHousePartyAsync(11);
            var guest = await AuthenticateAsync();

            // Act
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
            acceptReq.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            Assert.NotNull(postDataAfter.Data);
            Assert.Equal(11, postDataAfter.Data.AvailableSlots);
        }


        [Fact, Priority(1)]
        public async Task Accept_With_Empty_Request()
        {
            // Arrange
            var host = await AuthenticateAsync();
            var post = await CreateSampleHousePartyAsync(11);
            var guest = await AuthenticateAsync();

            // Act
            UpdateToken(host.JWT);

            var acceptUser = new AttendeeRequest
            {
            };

            var acceptReq = await TestClient.PostAsJsonAsync(ApiRoutes.EventAttendees.Accept, acceptUser);
            var getPostReqAfter = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", post.Id.ToString()));
            var postDataAfter = await getPostReqAfter.Content.ReadFromJsonAsync<Response<PostResponse>>();

            // Assert
            acceptReq.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            Assert.NotNull(postDataAfter.Data);
            Assert.Equal(11, postDataAfter.Data.AvailableSlots);
        }


        [Fact, Priority(1)]
        public async Task Accept_Attendee_StreetParty()
        {
            // Arrange
            var host = await AuthenticateAsync();
            var post = await CreateSamplePostAsync();
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
            acceptReq.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            Assert.NotNull(postDataBefore.Data);
            Assert.NotNull(postDataAfter.Data);

            Assert.Equal(0, postDataBefore.Data.AvailableSlots);
            Assert.Equal(0, postDataAfter.Data.AvailableSlots);
        }

        // REJECT ATTENDEE TEST --------------------------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact, Priority(1)]
        public async Task Reject_Attendee_Request_HouseParty()
        {
            // Arrange
            var host = await AuthenticateAsync();
            var post = await CreateSampleHousePartyAsync(15);
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

            var rejectReq = await TestClient.PostAsJsonAsync(ApiRoutes.EventAttendees.Reject, acceptUser);
            var getPostReqAfter = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", post.Id.ToString()));
            var postDataAfter = await getPostReqAfter.Content.ReadFromJsonAsync<Response<PostResponse>>();

            // Assert
            attendReq.StatusCode.Should().Be(HttpStatusCode.OK);
            rejectReq.StatusCode.Should().Be(HttpStatusCode.OK);

            Assert.NotNull(postDataBefore.Data);
            Assert.NotNull(postDataAfter.Data);

            Assert.Equal(15, postDataBefore.Data.AvailableSlots);
            Assert.Equal(15, postDataAfter.Data.AvailableSlots);
        }


        [Fact, Priority(1)]
        public async Task Reject_Accepted_Attendee_HouseParty()
        {
            // Arrange
            var host = await AuthenticateAsync();
            var post = await CreateSampleHousePartyAsync(10);
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

            // accept
            var acceptReq = await TestClient.PostAsJsonAsync(ApiRoutes.EventAttendees.Accept, acceptUser);
            var getPostReqBeforeReject = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", post.Id.ToString()));
            var postDataBeforeReject = await getPostReqBeforeReject.Content.ReadFromJsonAsync<Response<PostResponse>>();

            //reject
            var rejectReq = await TestClient.PostAsJsonAsync(ApiRoutes.EventAttendees.Reject, acceptUser);
            var getPostReqAfterReject = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", post.Id.ToString()));
            var postDataAfterReject = await getPostReqAfterReject.Content.ReadFromJsonAsync<Response<PostResponse>>();

            // Assert
            attendReq.StatusCode.Should().Be(HttpStatusCode.OK);
            acceptReq.StatusCode.Should().Be(HttpStatusCode.OK);
            rejectReq.StatusCode.Should().Be(HttpStatusCode.OK);

            Assert.NotNull(postDataBefore.Data);
            Assert.NotNull(postDataBeforeReject.Data);
            Assert.NotNull(postDataAfterReject.Data);

            Assert.Equal(10, postDataBefore.Data.AvailableSlots);
            Assert.Equal(9, postDataBeforeReject.Data.AvailableSlots);
            Assert.Equal(10, postDataAfterReject.Data.AvailableSlots);
        }


        [Fact, Priority(1)]
        public async Task Reject_Attendee_Who_DidNot_Request_HouseParty()
        {
            // Arrange
            var host = await AuthenticateAsync();
            var post = await CreateSampleHousePartyAsync(8);
            var guest = await AuthenticateAsync();

            // Act
            var getPostReqBefore = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", post.Id.ToString()));
            var postDataBefore = await getPostReqBefore.Content.ReadFromJsonAsync<Response<PostResponse>>();

            UpdateToken(host.JWT);

            var acceptUser = new AttendeeRequest
            {
                PostId = post.Id,
                AttendeeId = guest.UserId
            };

            var rejectReq = await TestClient.PostAsJsonAsync(ApiRoutes.EventAttendees.Reject, acceptUser);
            var getPostReqAfter = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", post.Id.ToString()));
            var postDataAfter = await getPostReqAfter.Content.ReadFromJsonAsync<Response<PostResponse>>();

            // Assert
            rejectReq.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            Assert.NotNull(postDataBefore.Data);
            Assert.NotNull(postDataAfter.Data);

            Assert.Equal(8, postDataBefore.Data.AvailableSlots);
            Assert.Equal(8, postDataAfter.Data.AvailableSlots);
        }


        [Fact, Priority(1)]
        public async Task Reject_With_EmptyRequest_HouseParty()
        {
            // Arrange
            var host = await AuthenticateAsync();
            var post = await CreateSamplePostAsync();
            var guest = await AuthenticateAsync();

            // Act
            var getPostReqBefore = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", post.Id.ToString()));
            var postDataBefore = await getPostReqBefore.Content.ReadFromJsonAsync<Response<PostResponse>>();

            UpdateToken(host.JWT);

            var acceptUser = new AttendeeRequest
            {
            };

            var rejectReq = await TestClient.PostAsJsonAsync(ApiRoutes.EventAttendees.Reject, acceptUser);
            var getPostReqAfter = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", post.Id.ToString()));
            var postDataAfter = await getPostReqAfter.Content.ReadFromJsonAsync<Response<PostResponse>>();

            // Assert
            rejectReq.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            Assert.NotNull(postDataBefore.Data);
            Assert.NotNull(postDataAfter.Data);

            Assert.Equal(0, postDataBefore.Data.AvailableSlots);
            Assert.Equal(0, postDataAfter.Data.AvailableSlots);
        }


        [Fact, Priority(1)]
        public async Task Reject_Attendee_StreetParty()
        {
            // Arrange
            var host = await AuthenticateAsync();
            var post = await CreateSamplePostAsync();
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

            var rejectReq = await TestClient.PostAsJsonAsync(ApiRoutes.EventAttendees.Reject, acceptUser);
            var getPostReqAfter = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", post.Id.ToString()));
            var postDataAfter = await getPostReqAfter.Content.ReadFromJsonAsync<Response<PostResponse>>();

            // Assert
            attendReq.StatusCode.Should().Be(HttpStatusCode.OK);
            rejectReq.StatusCode.Should().Be(HttpStatusCode.OK);

            Assert.NotNull(postDataBefore.Data);
            Assert.NotNull(postDataAfter.Data);

            Assert.Equal(0, postDataBefore.Data.AvailableSlots);
            Assert.Equal(0, postDataAfter.Data.AvailableSlots);
        }

// FETCH ALL ATTENDEE TEST ---------------------------------------------------------------------------------------------------------------------------------------------------
    
        [Fact, Priority(1)]
        public async Task Fetch_All_Attendee_HouseParty()
        {
            // Arrange
            var host = await AuthenticateAsync();
            var post = await CreateSampleHousePartyAsync(5);

            // Act

            // Request attend
            var guest1 = await AuthenticateAsync();            
            var attendReq1 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post.Id.ToString()), null);

            var guest2 = await AuthenticateAsync();
            var attendReq2 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post.Id.ToString()), null);

            var guest3 = await AuthenticateAsync();
            var attendReq3 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post.Id.ToString()), null);

            var guest4 = await AuthenticateAsync();
            var attendReq4 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post.Id.ToString()), null);

            var guest5 = await AuthenticateAsync();
            var attendReq5 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post.Id.ToString()), null);

            var guest6 = await AuthenticateAsync();
            var attendReq6 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post.Id.ToString()), null);

            UpdateToken(host.JWT);

            var acceptUser = new AttendeeRequest
            {
                PostId = post.Id,
                AttendeeId = guest1.UserId
            };
            
            // accept 3 requests
            var acceptReq1 = await TestClient.PostAsJsonAsync(ApiRoutes.EventAttendees.Accept, acceptUser);

            acceptUser.AttendeeId = guest2.UserId;
            var acceptReq2 = await TestClient.PostAsJsonAsync(ApiRoutes.EventAttendees.Accept, acceptUser);
            
            acceptUser.AttendeeId = guest3.UserId;
            var acceptReq3 = await TestClient.PostAsJsonAsync(ApiRoutes.EventAttendees.Accept, acceptUser);
                        
            var getPostReqAfterAccepted3 = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", post.Id.ToString()));
            var postDataAfter3 = await getPostReqAfterAccepted3.Content.ReadFromJsonAsync<Response<PostResponse>>();

            // accept all request
            acceptUser.AttendeeId = guest4.UserId;
            var acceptReq4 = await TestClient.PostAsJsonAsync(ApiRoutes.EventAttendees.Accept, acceptUser);

            acceptUser.AttendeeId = guest5.UserId;
            var acceptReq5 = await TestClient.PostAsJsonAsync(ApiRoutes.EventAttendees.Accept, acceptUser);

            acceptUser.AttendeeId = guest6.UserId;
            var acceptReq6 = await TestClient.PostAsJsonAsync(ApiRoutes.EventAttendees.Accept, acceptUser);

            var getPostReqAfterAccepted5 = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", post.Id.ToString()));
            var postDataAfterAll = await getPostReqAfterAccepted5.Content.ReadFromJsonAsync<Response<PostResponse>>();

            var getAllAccepted = await TestClient.GetAsync(ApiRoutes.EventAttendees.FetchAccepted + "?Id=" + post.Id.ToString());
            var acceptedParticipantsData = await getAllAccepted.Content.ReadFromJsonAsync<PagedResponse<EventParticipant>>();

            var getAllParticipants = await TestClient.GetAsync(ApiRoutes.EventAttendees.FetchAll+"?Id="+post.Id.ToString());
            var participantsData = await getAllParticipants.Content.ReadFromJsonAsync<PagedResponse<EventParticipant>>();

            // Assert
            attendReq1.StatusCode.Should().Be(HttpStatusCode.OK);
            attendReq2.StatusCode.Should().Be(HttpStatusCode.OK);
            attendReq3.StatusCode.Should().Be(HttpStatusCode.OK);
            attendReq4.StatusCode.Should().Be(HttpStatusCode.OK);
            attendReq5.StatusCode.Should().Be(HttpStatusCode.OK);
            attendReq6.StatusCode.Should().Be(HttpStatusCode.OK);
            acceptReq1.StatusCode.Should().Be(HttpStatusCode.OK);
            acceptReq2.StatusCode.Should().Be(HttpStatusCode.OK);
            acceptReq3.StatusCode.Should().Be(HttpStatusCode.OK);
            acceptReq4.StatusCode.Should().Be(HttpStatusCode.OK);
            acceptReq5.StatusCode.Should().Be(HttpStatusCode.OK);
            acceptReq6.StatusCode.Should().Be(HttpStatusCode.BadRequest);

           
            Assert.NotNull(postDataAfter3.Data);
            Assert.NotNull(participantsData.Data);
            Assert.NotNull(acceptedParticipantsData.Data);
            Assert.Equal(6, participantsData.Data.Count());
            Assert.Equal(5, acceptedParticipantsData.Data.Count());
            Assert.Equal(2, postDataAfter3.Data.AvailableSlots);
            Assert.Equal(0, postDataAfterAll.Data.AvailableSlots);
        }

// FETCH ALL Accepted ATTENDEE TEST ---------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact, Priority(1)]
        public async Task Fetch_All_Accepted_After_1User_Deled_HouseParty()
        {
            // Arrange
            var host = await AuthenticateAsync();
            var post = await CreateSampleHousePartyAsync(10);


            // Act
            // Request attend
            var guest1 = await AuthenticateAsync();
            var attendReq1 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post.Id.ToString()), null);

            var guest2 = await AuthenticateAsync();
            var attendReq2 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post.Id.ToString()), null);

            var guest3 = await AuthenticateAsync();
            var attendReq3 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post.Id.ToString()), null);

            UpdateToken(host.JWT);

            var acceptUser = new AttendeeRequest
            {
                PostId = post.Id,
                AttendeeId = guest1.UserId
            };

            // accept 3 requests
            var acceptReq1 = await TestClient.PostAsJsonAsync(ApiRoutes.EventAttendees.Accept, acceptUser);

            acceptUser.AttendeeId = guest2.UserId;
            var acceptReq2 = await TestClient.PostAsJsonAsync(ApiRoutes.EventAttendees.Accept, acceptUser);

            acceptUser.AttendeeId = guest3.UserId;
            var acceptReq3 = await TestClient.PostAsJsonAsync(ApiRoutes.EventAttendees.Accept, acceptUser);

            var getPostReqAfterAccepted3 = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", post.Id.ToString()));
            var postDataAfter3 = await getPostReqAfterAccepted3.Content.ReadFromJsonAsync<Response<PostResponse>>();

            // delete 1 user
            AuthenticateAdmin();
            var deleteUserReq = await TestClient.DeleteAsync(ApiRoutes.Users.Delete.Replace("{userId}", guest3.UserId));

            UpdateToken(host.JWT);

            // get final data
            var getPostReqAfterDelete = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", post.Id.ToString()));
            var postDataAfterDelete = await getPostReqAfterDelete.Content.ReadFromJsonAsync<Response<PostResponse>>();

            var getAllAccepted = await TestClient.GetAsync(ApiRoutes.EventAttendees.FetchAccepted + "?Id=" + post.Id.ToString());
            var acceptedParticipantsData = await getAllAccepted.Content.ReadFromJsonAsync<PagedResponse<EventParticipant>>();


            // Assert
            attendReq1.StatusCode.Should().Be(HttpStatusCode.OK);
            attendReq2.StatusCode.Should().Be(HttpStatusCode.OK);
            attendReq3.StatusCode.Should().Be(HttpStatusCode.OK);
            acceptReq1.StatusCode.Should().Be(HttpStatusCode.OK);
            acceptReq2.StatusCode.Should().Be(HttpStatusCode.OK);
            acceptReq3.StatusCode.Should().Be(HttpStatusCode.OK);
            deleteUserReq.StatusCode.Should().Be(HttpStatusCode.NoContent);

            Assert.NotNull(postDataAfter3.Data);
            Assert.Equal(7, postDataAfter3.Data.AvailableSlots);
            Assert.Equal(8, postDataAfterDelete.Data.AvailableSlots);
            Assert.Equal(2, acceptedParticipantsData.Data.Count());
        }



        [Fact, Priority(1)]
        public async Task Fetch_All_Accepted_StreeParty()
        {
            // Arrange
            var host = await AuthenticateAsync();
            var post = await CreateSamplePostAsync();


            // Act
            // Request attend
            var guest1 = await AuthenticateAsync();
            var attendReq1 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post.Id.ToString()), null);

            var guest2 = await AuthenticateAsync();
            var attendReq2 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post.Id.ToString()), null);

            var guest3 = await AuthenticateAsync();
            var attendReq3 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post.Id.ToString()), null);

            UpdateToken(host.JWT);

            var getPostReqAfterAccepted3 = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", post.Id.ToString()));
            var postDataAfter3 = await getPostReqAfterAccepted3.Content.ReadFromJsonAsync<Response<PostResponse>>();
            
            var getAllAccepted = await TestClient.GetAsync(ApiRoutes.EventAttendees.FetchAccepted + "?Id=" + post.Id.ToString());
            var acceptedParticipantsData = await getAllAccepted.Content.ReadFromJsonAsync<PagedResponse<EventParticipant>>();


            // Assert
            attendReq1.StatusCode.Should().Be(HttpStatusCode.OK);
            attendReq2.StatusCode.Should().Be(HttpStatusCode.OK);
            attendReq3.StatusCode.Should().Be(HttpStatusCode.OK);

            Assert.NotNull(postDataAfter3.Data);
            Assert.Equal(0, postDataAfter3.Data.AvailableSlots);
            Assert.Equal(3, acceptedParticipantsData.Data.Count());
        }


// FETCH ALL PENDING ATTENDEE TEST ---------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact, Priority(1)]
        public async Task Fetch_All_Pending_HouseParty()
        {
            // Arrange
            var host = await AuthenticateAsync();
            var post = await CreateSampleHousePartyAsync(10);


            // Act
            // Request attend
            var guest1 = await AuthenticateAsync();
            var attendReq1 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post.Id.ToString()), null);

            var guest2 = await AuthenticateAsync();
            var attendReq2 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post.Id.ToString()), null);

            var guest3 = await AuthenticateAsync();
            var attendReq3 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post.Id.ToString()), null);

            UpdateToken(host.JWT);

            var getPostReq = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", post.Id.ToString()));
            var postDataAfter3 = await getPostReq.Content.ReadFromJsonAsync<Response<PostResponse>>();

            var getAllAccepted = await TestClient.GetAsync(ApiRoutes.EventAttendees.FetchAccepted + "?Id=" + post.Id.ToString());
            var acceptedParticipantsData = await getAllAccepted.Content.ReadFromJsonAsync<PagedResponse<EventParticipant>>();
            var getAllPending = await TestClient.GetAsync(ApiRoutes.EventAttendees.FetchPending + "?Id=" + post.Id.ToString());
            var pendingParticipantsData = await getAllPending.Content.ReadFromJsonAsync<PagedResponse<EventParticipant>>();


            // Assert
            attendReq1.StatusCode.Should().Be(HttpStatusCode.OK);
            attendReq2.StatusCode.Should().Be(HttpStatusCode.OK);
            attendReq3.StatusCode.Should().Be(HttpStatusCode.OK);

            Assert.NotNull(postDataAfter3.Data);
            Assert.Equal(10, postDataAfter3.Data.AvailableSlots);
            Assert.Empty(acceptedParticipantsData.Data);
            Assert.Equal(3, pendingParticipantsData.Data.Count());
        }


        [Fact, Priority(1)]
        public async Task Fetch_All_Pending_StreetParty_Always0()
        {
            // Arrange
            var host = await AuthenticateAsync();
            var post = await CreateSamplePostAsync();


            // Act
            // Request attend
            var guest1 = await AuthenticateAsync();
            var attendReq1 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post.Id.ToString()), null);

            var guest2 = await AuthenticateAsync();
            var attendReq2 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post.Id.ToString()), null);

            var guest3 = await AuthenticateAsync();
            var attendReq3 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", post.Id.ToString()), null);

            UpdateToken(host.JWT);

            var getPostReq = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", post.Id.ToString()));
            var postDataAfter3 = await getPostReq.Content.ReadFromJsonAsync<Response<PostResponse>>();

            var getAllAccepted = await TestClient.GetAsync(ApiRoutes.EventAttendees.FetchPending + "?Id=" + post.Id.ToString());
            var acceptedParticipantsData = await getAllAccepted.Content.ReadFromJsonAsync<PagedResponse<EventParticipant>>();


            // Assert
            attendReq1.StatusCode.Should().Be(HttpStatusCode.OK);
            attendReq2.StatusCode.Should().Be(HttpStatusCode.OK);
            attendReq3.StatusCode.Should().Be(HttpStatusCode.OK);

            Assert.NotNull(postDataAfter3.Data);
            Assert.Equal(0, postDataAfter3.Data.AvailableSlots);
            Assert.Empty(acceptedParticipantsData.Data);
        }

    }
}
