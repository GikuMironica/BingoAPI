using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.Report;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.Report;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Priority;

namespace Bingo.IntegrationTests.ReportControllerTest
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class ReportControllerTest : ReportIntegrationTest
    {
// CREATE POST-REPORT TEST

        [Fact, Priority(5)]
        public async Task Create_Valid_PostReport()
        {
            // Arrange
            var reported = await AuthenticateAsync();
            var post = await CreateSamplePostAsync();

            // Act
            var report = new CreateReportRequest
            {
                Message = "ShitboxHahaha",
                Reason = "I dont like it",
                PostId = post.PostId
            };

            var reporter1 = await AuthenticateAsync();
            var reportReq1 = await TestClient.PostAsJsonAsync(ApiRoutes.Reports.Create, report);
            var responseData1 = await reportReq1.Content.ReadFromJsonAsync<Response<CreateReportResponse>>();

            var reporter2 = await AuthenticateAsync();
            var reportReq2 = await TestClient.PostAsJsonAsync(ApiRoutes.Reports.Create, report);
            var responseData2 = await reportReq2.Content.ReadFromJsonAsync<Response<CreateReportResponse>>();

            AuthenticateAdmin();
            var getReportsReq = await TestClient.GetAsync(ApiRoutes.Reports.GetAll.Replace("{userId}", reported.UserId));
            var getReportsData = await getReportsReq.Content.ReadFromJsonAsync<Response<List<ReportResponse>>>();

            // Assert
            reportReq1.StatusCode.Should().Be(HttpStatusCode.Created);
            reportReq2.StatusCode.Should().Be(HttpStatusCode.Created);
            getReportsReq.StatusCode.Should().Be(HttpStatusCode.OK);
            Assert.NotNull(getReportsData.Data);
            Assert.Equal(2, getReportsData.Data.Count);
        }


        [Fact, Priority(5)]
        public async Task Create_Empty_PostReport()
        {
            // Arrange
            var reported = await AuthenticateAsync();
            var post = await CreateSamplePostAsync();

            // Act
            var report = new CreateReportRequest
            {

            };

            var reporter1 = await AuthenticateAsync();
            var reportReq1 = await TestClient.PostAsJsonAsync(ApiRoutes.Reports.Create, report);

            // Assert
            reportReq1.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }


        [Fact, Priority(5)]
        public async Task Create_HalfEmpty_PostReport()
        {
            // Arrange
            var reported = await AuthenticateAsync();
            var post = await CreateSamplePostAsync();

            // Act
            var report = new CreateReportRequest
            {
                Message = "ShitboxHahaha",
                Reason = "I dont like it"
            };

            var reporter1 = await AuthenticateAsync();
            var reportReq1 = await TestClient.PostAsJsonAsync(ApiRoutes.Reports.Create, report);

            // Assert
            reportReq1.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }


        [Fact, Priority(5)]
        public async Task Create_UnexistentPostId_PostReport()
        {
            // Arrange
            var reported = await AuthenticateAsync();

            // Act
            var report = new CreateReportRequest
            {
                Message = "ShitboxHahaha",
                Reason = "I dont like it",
                PostId = 99999999
            };

            var reporter1 = await AuthenticateAsync();
            var reportReq1 = await TestClient.PostAsJsonAsync(ApiRoutes.Reports.Create, report);

            // Assert
            reportReq1.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

// DELETE / DELETE ALL TEST

        [Fact, Priority(5)]
        public async Task Delete_Report_When_Exists()
        {
            // Arrange
            var user = await AuthenticateAsync();
            var post = await CreateSamplePostAsync();
            var reporter = await AuthenticateAsync();

            // Act
            var report = new CreateReportRequest
            {
                Message = "ShitboxHahaha",
                Reason = "I dont like it",
                PostId = post.PostId
            };
            var reportReq = await TestClient.PostAsJsonAsync(ApiRoutes.Reports.Create, report);
            var responseData = await reportReq.Content.ReadFromJsonAsync<Response<CreateReportResponse>>();

            AuthenticateAdmin();
            var deleteReq = await TestClient.DeleteAsync(ApiRoutes.Reports.Delete.Replace("{reportId}", responseData.Data.Id.ToString()));
            var tryGetReq = await TestClient.GetAsync(ApiRoutes.Reports.Get.Replace("{reportId}", responseData.Data.Id.ToString()));

            // Assert
            reportReq.StatusCode.Should().Be(HttpStatusCode.Created);
            deleteReq.StatusCode.Should().Be(HttpStatusCode.NoContent);
            tryGetReq.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }


        [Fact, Priority(5)]
        public async Task Delete_Report_Doesnt_Exists()
        {
            // Arrange
            var user = await AuthenticateAsync();
            var reporter = await AuthenticateAsync();

            // Act

            AuthenticateAdmin();
            var deleteReq = await TestClient.DeleteAsync(ApiRoutes.Reports.Delete.Replace("{reportId}", "99990909"));

            // Assert
            deleteReq.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }


        [Fact, Priority(5)]
        public async Task Delete_UnAuthorized_When_Exists()
        {
            // Arrange
            var user = await AuthenticateAsync();
            var post = await CreateSamplePostAsync();
            var reporter = await AuthenticateAsync();

            // Act
            var report = new CreateReportRequest
            {
                Message = "ShitboxHahaha",
                Reason = "I dont like it",
                PostId = post.PostId
            };
            var reportReq = await TestClient.PostAsJsonAsync(ApiRoutes.Reports.Create, report);
            var responseData = await reportReq.Content.ReadFromJsonAsync<Response<CreateReportResponse>>();

            var deleteReq = await TestClient.DeleteAsync(ApiRoutes.Reports.Delete.Replace("{reportId}", responseData.Data.Id.ToString()));
            var tryGetReq = await TestClient.GetAsync(ApiRoutes.Reports.Get.Replace("{reportId}", responseData.Data.Id.ToString()));

            // Assert
            reportReq.StatusCode.Should().Be(HttpStatusCode.Created);
            deleteReq.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            tryGetReq.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }




        [Fact, Priority(5)]
        public async Task Delete_Multiple_Reports_When_Exist()
        {
            // Arrange
            var reported = await AuthenticateAsync();
            var post = await CreateSamplePostAsync();

            // Act
            var report = new CreateReportRequest
            {
                Message = "To be deleted anyawys",
                Reason = "I dont like it",
                PostId = post.PostId
            };

            var reporter1 = await AuthenticateAsync();
            var reportReq1 = await TestClient.PostAsJsonAsync(ApiRoutes.Reports.Create, report);
            var responseData1 = await reportReq1.Content.ReadFromJsonAsync<Response<CreateReportResponse>>();

            var reporter2 = await AuthenticateAsync();
            var reportReq2 = await TestClient.PostAsJsonAsync(ApiRoutes.Reports.Create, report);
            var responseData2 = await reportReq2.Content.ReadFromJsonAsync<Response<CreateReportResponse>>();

            var reporter3 = await AuthenticateAsync();
            var reportReq3 = await TestClient.PostAsJsonAsync(ApiRoutes.Reports.Create, report);
            var responseData3 = await reportReq3.Content.ReadFromJsonAsync<Response<CreateReportResponse>>();

            AuthenticateAdmin();
            var getAllReq = await TestClient.GetAsync(ApiRoutes.Reports.GetAll.Replace("{userId}", reported.UserId));
            var getReportsData = await getAllReq.Content.ReadFromJsonAsync<Response<List<ReportResponse>>>();

            var deleteReportsReq = await TestClient.DeleteAsync(ApiRoutes.Reports.DeleteAll.Replace("{userId}", reported.UserId));

            // Assert
            reportReq1.StatusCode.Should().Be(HttpStatusCode.Created);
            reportReq2.StatusCode.Should().Be(HttpStatusCode.Created);
            reportReq3.StatusCode.Should().Be(HttpStatusCode.Created);

            getAllReq.StatusCode.Should().Be(HttpStatusCode.OK);
            Assert.NotNull(getReportsData.Data);
            Assert.Equal(3, getReportsData.Data.Count);

            deleteReportsReq.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }



        [Fact, Priority(5)]
        public async Task Delete_Multiple_Reports_When_Doesnt_Exist()
        {
            // Arrange
            var reported = await AuthenticateAsync();
            var post = await CreateSamplePostAsync();

            // Act
            AuthenticateAdmin();
            var getAllReq = await TestClient.GetAsync(ApiRoutes.Reports.GetAll.Replace("{userId}", reported.UserId));

            var deleteReportsReq = await TestClient.DeleteAsync(ApiRoutes.Reports.DeleteAll.Replace("{userId}", reported.UserId));

            // Assert
            getAllReq.StatusCode.Should().Be(HttpStatusCode.NoContent);
            deleteReportsReq.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }


        [Fact, Priority(5)]
        public async Task Delete_All_UnAuthorized_When_Exists()
        {
            // Arrange
            var user = await AuthenticateAsync();
            var post = await CreateSamplePostAsync();
            var reporter = await AuthenticateAsync();

            // Act
            var report = new CreateReportRequest
            {
                Message = "ShitboxHahaha",
                Reason = "I dont like it",
                PostId = post.PostId
            };
            var reportReq = await TestClient.PostAsJsonAsync(ApiRoutes.Reports.Create, report);
            var responseData = await reportReq.Content.ReadFromJsonAsync<Response<CreateReportResponse>>();

            var deleteReq = await TestClient.DeleteAsync(ApiRoutes.Reports.DeleteAll.Replace("{userId}", user.UserId));

            AuthenticateAdmin();
            var tryGetReq = await TestClient.GetAsync(ApiRoutes.Reports.GetAll.Replace("{userId}", user.UserId));
            var getReportsData = await tryGetReq.Content.ReadFromJsonAsync<Response<List<ReportResponse>>>();

            // Assert
            reportReq.StatusCode.Should().Be(HttpStatusCode.Created);
            deleteReq.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            tryGetReq.StatusCode.Should().Be(HttpStatusCode.OK);

            Assert.NotNull(responseData.Data);
            Assert.Single(getReportsData.Data);
        }
    }
}
