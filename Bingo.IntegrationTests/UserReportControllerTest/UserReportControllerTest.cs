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

        public static int _reportId { get; set; }

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
                Reason = 1,
                ReportedUserId = reported.UserId
            };
            var reportResponse = await TestClient.PostAsJsonAsync(ApiRoutes.UserReports.Create, report);
            var response = await reportResponse.Content.ReadFromJsonAsync<Response<CreateUserReportResponse>>();

            AuthenticateAdmin();
            var getReportRes = await TestClient.GetAsync(ApiRoutes.UserReports.Get.Replace("{reportId}", response.Data.Id.ToString()));
            var getReportData = await getReportRes.Content.ReadFromJsonAsync<Response<UserReportResponse>>();
            _reportId = response.Data.Id;

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
                Reason = 2
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
                Reason = 4,
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
        public async Task Z_Delete_UserReport_Authorized()
        {
            // Arrange
            var host = await AuthenticateAsync();
            var reporter = await AuthenticateAsync();

            var report = new ReportUserRequest
            {
                Message = "He is a nutbag",
                Reason = 5,
                ReportedUserId = host.UserId
            };


            // Act
            var reportResponse = await TestClient.PostAsJsonAsync(ApiRoutes.UserReports.Create, report);
            var response = await reportResponse.Content.ReadFromJsonAsync<Response<CreateUserReportResponse>>();

            AuthenticateAdmin();
            var deleteResponse = await TestClient.DeleteAsync(ApiRoutes.UserReports.Delete.Replace("{reportId}", response.Data.Id.ToString()));


            // Assert
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }


        [Fact, Priority(15)]
        public async Task Z_Delete_UserReport_EmptyReq()
        {
            // Arrange
            var host = await AuthenticateAsync();
            var reporter = await AuthenticateAsync();

            var report = new ReportUserRequest
            {
                Message = "He is a nutbag",
                Reason = 1,
                ReportedUserId = host.UserId
            };


            // Act
            var reportResponse = await TestClient.PostAsJsonAsync(ApiRoutes.UserReports.Create, report);
            var response = await reportResponse.Content.ReadFromJsonAsync<Response<CreateUserReportResponse>>();

            AuthenticateAdmin();
            var deleteResponse = await TestClient.DeleteAsync(ApiRoutes.UserReports.Delete.Replace("{reportId}", "994359354"));


            // Assert
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }


        [Fact, Priority(15)]
        public async Task Z_Delete_UserReport_Unauthorized()
        {
            // Arrange
            var host = await AuthenticateAsync();
            var reporter = await AuthenticateAsync();

            var report = new ReportUserRequest
            {
                Message = "He is a nutbag",
                Reason = 2,
                ReportedUserId = host.UserId
            };


            // Act
            var reportResponse = await TestClient.PostAsJsonAsync(ApiRoutes.UserReports.Create, report);
            var response = await reportResponse.Content.ReadFromJsonAsync<Response<CreateUserReportResponse>>();

            var deleteResponse = await TestClient.DeleteAsync(ApiRoutes.UserReports.Delete.Replace("{reportId}", response.Data.Id.ToString()));

            // Assert
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

// GETALL REPORTS TEST -----------------------------------------------------------------------------------------------------------------------------------------------------
        
        [Fact, Priority(20)]
        public async Task Get_All_UserReports_WhenExit()
        {
            // Arrange
            var reported = await AuthenticateAsync();
            var reporter = await AuthenticateAsync();

            var report = new ReportUserRequest
            {
                Message = "He is a nutbag",
                Reason = 3,
                ReportedUserId = reported.UserId
            };
            var reportResponse1 = await TestClient.PostAsJsonAsync(ApiRoutes.UserReports.Create, report);
            var response1 = await reportResponse1.Content.ReadFromJsonAsync<Response<CreateUserReportResponse>>();

            var reporter2 = await AuthenticateAsync();
            var reportResponse2 = await TestClient.PostAsJsonAsync(ApiRoutes.UserReports.Create, report);
            var response2 = await reportResponse2.Content.ReadFromJsonAsync<Response<CreateUserReportResponse>>();

            var reporter3 = await AuthenticateAsync();
            var reportResponse3 = await TestClient.PostAsJsonAsync(ApiRoutes.UserReports.Create, report);
            var response3 = await reportResponse3.Content.ReadFromJsonAsync<Response<CreateUserReportResponse>>();

            // Act
            AuthenticateAdmin();
            var getAllResponse = await TestClient.GetAsync(ApiRoutes.UserReports.GetAll.Replace("{userId}", reported.UserId));
            var data = await getAllResponse.Content.ReadFromJsonAsync<Response<List<UserReportResponse>>>();

            // Assert
            reportResponse1.StatusCode.Should().Be(HttpStatusCode.Created);
            reportResponse2.StatusCode.Should().Be(HttpStatusCode.Created);
            reportResponse3.StatusCode.Should().Be(HttpStatusCode.Created);

            Assert.NotNull(response1.Data);
            Assert.NotNull(response2.Data);
            Assert.NotNull(response3.Data);

            Assert.Equal(3, data.Data.Count);
        }


        [Fact, Priority(20)]
        public async Task Get_All_UserReports_WhenDoesntExit()
        {
            // Arrange
            var reported = await AuthenticateAsync();

            // Act
            AuthenticateAdmin();
            var getAllResponse = await TestClient.GetAsync(ApiRoutes.UserReports.GetAll.Replace("{userId}", reported.UserId));

            // Assert
            getAllResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }
    }
}
