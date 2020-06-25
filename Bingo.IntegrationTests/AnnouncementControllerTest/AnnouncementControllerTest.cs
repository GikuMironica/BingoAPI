using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.Announcement;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.Announcement;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Priority;

namespace Bingo.IntegrationTests.AnnouncementControllerTest
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class AnnouncementControllerTest : AnnouncementIntegrationTest
    {

        private static string _userId;
        private static string _announcementId;

// CREATE ANNOUNCEMENTS TEST --------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact, Priority(1)]
        public async Task A_Create_Announcement_WithValidPostId()
        {
            // Arrange
            var result = await AuthenticateAsync();
            _userId = result.UserId;
            var createPostResult = await CreateSamplePostAsync();
            var newAnnouncement = new CreateAnnouncementRequest
            {
                PostId = createPostResult.Id,
                Message = "This is a sample Announcement 😋😎😎😶😴🤔😃🤗😢😍🍣🥗☪💫🔯🈚🆑🆎🆎㊗ for a sample post"
            };

            // Act
            var createAnnouncementResponse = await TestClient.PostAsJsonAsync(ApiRoutes.Announcements.Create, newAnnouncement);
            var announcement = await createAnnouncementResponse.Content.ReadFromJsonAsync<Response<CreateAnnouncementResponse>>();
            var getAnnouncementResponse = await TestClient.GetAsync(ApiRoutes.Announcements.Get.Replace("{announcementId}", announcement.Data.Id.ToString()));
            _announcementId = announcement.Data.Id.ToString();

            // Assert
            Assert.NotEqual(HttpStatusCode.InternalServerError, createAnnouncementResponse.StatusCode);
            Assert.True(createAnnouncementResponse.IsSuccessStatusCode);
            Assert.NotEqual(HttpStatusCode.InternalServerError, getAnnouncementResponse.StatusCode);
            Assert.True(getAnnouncementResponse.IsSuccessStatusCode);
            Assert.NotNull(announcement.Data);
            Assert.Equal("This is a sample Announcement 😋😎😎😶😴🤔😃🤗😢😍🍣🥗☪💫🔯🈚🆑🆎🆎㊗ for a sample post", announcement.Data.Message);
            Assert.NotEqual(0, announcement.Data.Timestamp);
            Assert.Equal(createPostResult.Id, announcement.Data.PostId);

        }


        [Fact, Priority(2)]
        public async Task A_Create_Announcement_WithInvalid_Data()
        {
            // Arrange
            var result = await AuthenticateAsync();
            var createPostResult = await CreateSamplePostAsync();
            var newAnnouncement = new CreateAnnouncementRequest
            {
                
            };

            // Act
            var createAnnouncementResponse = await TestClient.PostAsJsonAsync(ApiRoutes.Announcements.Create, newAnnouncement);
            var announcement = await createAnnouncementResponse.Content.ReadFromJsonAsync<Response<CreateAnnouncementResponse>>();

            // Assert
            Assert.NotEqual(HttpStatusCode.InternalServerError, createAnnouncementResponse.StatusCode);
            Assert.True(!createAnnouncementResponse.IsSuccessStatusCode);
            createAnnouncementResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact, Priority(2)]
        public async Task A_Create_Announcement_WithInvalid_PostId()
        {
            // Arrange
            var result = await AuthenticateAsync();
            var createPostResult = await CreateSamplePostAsync();
            var newAnnouncement = new CreateAnnouncementRequest
            {
                PostId = 999999999,
                Message = "This is a sample A😎😎😶😴🤔😃🤗😢😍🍣🥗☪💫🔯🈚🆑🆎🆎㊗ for a sample post"
            };

            // Act
            var createAnnouncementResponse = await TestClient.PostAsJsonAsync(ApiRoutes.Announcements.Create, newAnnouncement);

            // Assert
            Assert.NotEqual(HttpStatusCode.InternalServerError, createAnnouncementResponse.StatusCode);
            Assert.True(!createAnnouncementResponse.IsSuccessStatusCode);
            createAnnouncementResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

// UPDATE ANNOUNCEMENT TEST -------------------------------------------------------------------------------------------------------------------------------------------------


        [Fact, Priority(10)]
        public async Task B_Update_Announcement_WithValid_Data()
        {

        }
    }
}
