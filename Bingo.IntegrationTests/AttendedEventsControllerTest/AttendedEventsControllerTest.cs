using Bingo.Contracts.V1;
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
    }
}
