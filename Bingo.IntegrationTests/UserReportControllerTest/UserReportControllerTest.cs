using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.Report;
using Bingo.Contracts.V1.Requests.UserReport;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.UserReport;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Priority;

namespace Bingo.IntegrationTests.UserReportControllerTest
{

    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class UserReportControllerTest : IntegrationTest
    {
// CREATE REPORT TEST ------------------------------------------------------------------------------------------------------------------------------------------------------
        
        [Fact, Priority(5)]
        public async Task A_Create_UserReport_Valid()
        {
            // Arrange
            var reported = await AuthenticateAsync();
            var reporter = await AuthenticateAsync();


            // Act
            var report = new ReportUserRequest
            {
                Message = "He is a nutbag",
                Reason = "Spam",
                ReportedUserId = reported.UserId
            };
            var reportResponse = await TestClient.PostAsJsonAsync(ApiRoutes.UserReports.Create, report);
            var response = await reportResponse.Content.ReadFromJsonAsync<Response<CreateUserReportResponse>>();

            AuthenticateAdmin();
            var getReportRes = await TestClient.GetAsync(ApiRoutes.UserReports.Get.Replace("{reportId}", response.Data.Id.ToString()));
            var getReportData = await getReportRes.Content.ReadFromJsonAsync<Response<UserReportResponse>>();


            // Assert
            reportResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            getReportRes.StatusCode.Should().Be(HttpStatusCode.OK);
            Assert.NotNull(response.Data);
            Assert.Equal(reported.UserId, response.Data.ReportedUserId);
            Assert.Equal(reporter.UserId, response.Data.ReporterId);
            Assert.NotEqual(0, response.Data.Timestamp);

            Assert.NotNull(getReportData.Data);
            Assert.Equal("He is a nutbag", getReportData.Data.Message);
            Assert.Equal("Spam", getReportData.Data.Reason);

        }



        [Fact, Priority(5)]
        public async Task A_Create_UserReport_Empty_Request()
        {
            // Arrange
            var reported = await AuthenticateAsync();
            var reporter = await AuthenticateAsync();


            // Act
            var report = new ReportUserRequest
            {
            };
            var reportResponse = await TestClient.PostAsJsonAsync(ApiRoutes.UserReports.Create, report);
            var response = await reportResponse.Content.ReadFromJsonAsync<Response<CreateUserReportResponse>>();
            AuthenticateAdmin();


            // Assert
            reportResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            Assert.Null(response.Data);
        }


        [Fact, Priority(5)]
        public async Task A_Create_UserReport_HalfEmpty_Request()
        {
            // Arrange
            var reported = await AuthenticateAsync();
            var reporter = await AuthenticateAsync();


            // Act
            var report = new ReportUserRequest
            {
                Message = "haha",
                Reason = "gsdgs"
            };
            var reportResponse = await TestClient.PostAsJsonAsync(ApiRoutes.UserReports.Create, report);
            var response = await reportResponse.Content.ReadFromJsonAsync<Response<CreateUserReportResponse>>();
            AuthenticateAdmin();


            // Assert
            reportResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            Assert.Null(response.Data);
        }


        [Fact, Priority(5)]
        public async Task A_Create_UserReport_Multiple_Requests_Within_1_Week()
        {
            // Arrange
            var reported = await AuthenticateAsync();
            var reporter = await AuthenticateAsync();


            // Act
            var report = new ReportUserRequest
            {
                Message = "He is a nutbag",
                Reason = "Spam",
                ReportedUserId = reported.UserId
            };
            var report1Response = await TestClient.PostAsJsonAsync(ApiRoutes.UserReports.Create, report);
            var report2Response = await TestClient.PostAsJsonAsync(ApiRoutes.UserReports.Create, report);
            var response1 = await report1Response.Content.ReadFromJsonAsync<Response<CreateUserReportResponse>>();
            var response2 = await report2Response.Content.ReadFromJsonAsync<Response<CreateUserReportResponse>>();

            AuthenticateAdmin();


            // Assert
            report1Response.StatusCode.Should().Be(HttpStatusCode.Created);
            report2Response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

            Assert.Null(response2.Data);
            Assert.NotNull(response1.Data);
        }

// DELETE REPORT TEST --------------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact, Priority(15)]
        public async Task Delete_UserReport_Authorized()
        {
            // Arrange
            AuthenticateAdmin();


            // Act



            // Assert
        }
    }
}
