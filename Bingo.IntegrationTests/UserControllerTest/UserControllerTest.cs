using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.Announcement;
using Bingo.Contracts.V1.Requests.EventAttendee;
using Bingo.Contracts.V1.Requests.Report;
using Bingo.Contracts.V1.Requests.User;
using Bingo.Contracts.V1.Requests.UserReport;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.Post;
using Bingo.Contracts.V1.Responses.User;
using BingoAPI.Models;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Priority;

namespace Bingo.IntegrationTests.UserControllerTest
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class UserControllerTest : UserIntegrationTest
    {

        public static string _updatedUser { get; set; }

        // Update USER TEST ---------------------------------------------------------------------------------------------------------------------------------------------

        [Fact, Priority(-10)]
        public async Task A_Update_User_With_Valid_Data()
        {
            // Arrange
            var authResponse = await AuthenticateAsync();
            _updatedUser = authResponse.UserId;


            // Act
            var updateUser = new UpdateUserRequest
            {
                Description = "LolMaNiggaas",
                FirstName = "TestUser"
            };
            var getInitialUserResponse = await TestClient.GetAsync(ApiRoutes.Users.Get.Replace("{userId}", _updatedUser));
            var result = await UpdateUserAsync(updateUser, _updatedUser);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("LolMaNiggaas", result.Description);
            Assert.Equal("TestUser", result.FirstName);
            Assert.NotEqual("Test", result.LastName);
            Assert.NotNull(result.LastName);
        }

        [Fact, Priority(10)]
        public async Task B_Update_User_With_InValid_Data()
        {
            // Arrange
            AuthenticateAdmin();

            // Act
            var updateUser = new UpdateUserRequest
            {
               
            };
            var result = await UpdateUserAsync(updateUser, _updatedUser);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("LolMaNiggaas", result.Description);
            Assert.Equal("TestUser", result.FirstName);
            Assert.NotEqual("Test", result.LastName);
            Assert.NotNull(result.LastName);
        }



        [Fact, Priority(20)]
        public async Task C_Delete_User_Bad_Id()
        {
            // Arrange
            AuthenticateAdmin();


            // Act
            var deleteResponse = await TestClient.DeleteAsync(ApiRoutes.Users.Delete.Replace("{userId}", _updatedUser));
            var getResponse = await TestClient.GetAsync(ApiRoutes.Users.Get.Replace("{userId}", _updatedUser));

            // Assert
            Assert.NotEqual(HttpStatusCode.InternalServerError, deleteResponse.StatusCode);
            Assert.NotEqual(HttpStatusCode.InternalServerError, getResponse.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        }


        [Fact, Priority(20)]
        public async Task C_Delete_User_With_RelevantData()
        {
            // Arrange
            var user = await AuthenticateAsync();
            var post = await CreateSamplePostAsync();

            // add announcement
            var newAnnouncement = new CreateAnnouncementRequest
            {
                PostId = post.PostId,
                Message = "This is a sample Announcement 😋😎😎😶😴🤔😃🤗😢😍🍣🥗☪💫🔯🈚🆑🆎🆎㊗ for a sample post"
            };

            // report user post
            var report = new CreateReportRequest
            {
                Message = "ShitboxHahaha",
                Reason = "I dont like it",
                PostId = post.PostId
            };

            // report user
            var reportUser = new ReportUserRequest
            {
                Message = "He is a nutbag",
                Reason = "Spam",
                ReportedUserId = user.UserId
            };


            // attend an event
            var host = await AuthenticateAsync();
            var party = await CreateSamplePostAsync();

            var attendReq = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", party.PostId.ToString()), null);
            var getPostReqBefore = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", party.PostId.ToString()));
            var postDataBefore = await getPostReqBefore.Content.ReadFromJsonAsync<Response<PostResponse>>();

            UpdateToken(user.JWT);
                      

            // Act
            var reportResponse = await TestClient.PostAsJsonAsync(ApiRoutes.UserReports.Create, reportUser);
            var reportReq1 = await TestClient.PostAsJsonAsync(ApiRoutes.Reports.Create, report);
            var createAnnouncementResponse = await TestClient.PostAsJsonAsync(ApiRoutes.Announcements.Create, newAnnouncement);
            var deleteResponse = await TestClient.DeleteAsync(ApiRoutes.Users.Delete.Replace("{userId}", user.UserId));
            var getResponse = await TestClient.GetAsync(ApiRoutes.Users.Get.Replace("{userId}", user.UserId));


            // Assert
            Assert.NotEqual(HttpStatusCode.InternalServerError, deleteResponse.StatusCode);
            Assert.NotEqual(HttpStatusCode.InternalServerError, getResponse.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
            Assert.NotNull(postDataBefore.Data);
            reportResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            reportReq1.StatusCode.Should().Be(HttpStatusCode.Created);
            createAnnouncementResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            attendReq.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
