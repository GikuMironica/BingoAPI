using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.Rating;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.Rating;
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

namespace Bingo.IntegrationTests.RatingsTest
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class RatingsControllerTest : RatingsIntegrationTest
    {
        // CREATE RATING TEST--------------------------------------------------------------------------------------------------------------------------------------------------------

        public static int _ratingId { get; set; }

        [Fact, Priority(-10)]
        public async Task A_Create_Valid_Rating()
        {
            // Arrange
            var hostAuthResult = await AuthenticateAsync();
            var createEvent = await CreateSamplePostAsync();
            var guestAuthResult = await AuthenticateAsync();

            // Act
            var rating = new CreateRatingRequest
            {
                Feedback = "Good event 👨‍🦰🧑",
                PostId = createEvent.Id,
                UserId = hostAuthResult.UserId,
                Rate = 4
            };

            var attentReq = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", createEvent.Id.ToString()), null);
            var rateReq = await TestClient.PostAsJsonAsync(ApiRoutes.Ratings.Create, rating);
            var response = await rateReq.Content.ReadFromJsonAsync<Response<CreateRatingResponse>>();
            _ratingId = response.Data.Id;
            var getReq = await TestClient.GetAsync(ApiRoutes.Ratings.Get.Replace("{ratingId}", response.Data.Id.ToString()));

            // Assert
            Assert.NotEqual(HttpStatusCode.InternalServerError, attentReq.StatusCode);
            Assert.NotEqual(HttpStatusCode.InternalServerError, rateReq.StatusCode);
            attentReq.StatusCode.Should().Be(HttpStatusCode.OK);
            Assert.True(rateReq.IsSuccessStatusCode);
            Assert.True(getReq.IsSuccessStatusCode);

            Assert.Equal("Good event 👨‍🦰🧑", response.Data.Feedback);
            Assert.Equal(4, response.Data.Rate);
        }


        [Fact, Priority(1)]
        public async Task A_Create_InValid_Rating()
        {
            // Arrange
            var hostAuthResult = await AuthenticateAsync();
            var createEvent = await CreateSamplePostAsync();
            var guestAuthResult = await AuthenticateAsync();

            // Act
            var rating = new CreateRatingRequest
            {
                Feedback = "Good event 👨‍🦰🧑",
                UserId = hostAuthResult.UserId,
                Rate = 4
            };

            var rateReq = await TestClient.PostAsJsonAsync(ApiRoutes.Ratings.Create, rating);
            var response = await rateReq.Content.ReadFromJsonAsync<Response<CreateRatingResponse>>();
            var getReq = await TestClient.GetAsync(ApiRoutes.Ratings.Get.Replace("{ratingId}", "99994235"));

            // Assert
            Assert.NotEqual(HttpStatusCode.InternalServerError, rateReq.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, rateReq.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, getReq.StatusCode);
        }


        [Fact, Priority(1)]
        public async Task A_Create_NoData_Rating()
        {
            // Arrange
            var hostAuthResult = await AuthenticateAsync();
            var createEvent = await CreateSamplePostAsync();
            var guestAuthResult = await AuthenticateAsync();

            // Act
            var rating = new CreateRatingRequest
            {
            };

            var rateReq = await TestClient.PostAsJsonAsync(ApiRoutes.Ratings.Create, rating);
            var response = await rateReq.Content.ReadFromJsonAsync<Response<CreateRatingResponse>>();

            // Assert
            Assert.NotEqual(HttpStatusCode.InternalServerError, rateReq.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, rateReq.StatusCode);
            Assert.Null(response.Data);
        }


        [Fact, Priority(10)]
        public async Task B_Create_MultipleRatings_On_SameEventAndHost()
        {
            // Arrange
            var hostAuthResult = await AuthenticateAsync();
            var createEvent = await CreateSamplePostAsync();
            var guestAuthResult = await AuthenticateAsync();

            // Act
            var rating = new CreateRatingRequest
            {
                Feedback = "Good event 👨‍🦰🧑",
                PostId = createEvent.Id,
                UserId = hostAuthResult.UserId,
                Rate = 4
            };

            var attentReq = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", createEvent.Id.ToString()), null);
            var rateReq = await TestClient.PostAsJsonAsync(ApiRoutes.Ratings.Create, rating);
            var rateReq2 = await TestClient.PostAsJsonAsync(ApiRoutes.Ratings.Create, rating);
            var response = await rateReq.Content.ReadFromJsonAsync<Response<CreateRatingResponse>>();

            // Assert
            Assert.NotEqual(HttpStatusCode.InternalServerError, attentReq.StatusCode);
            Assert.NotEqual(HttpStatusCode.InternalServerError, rateReq.StatusCode);
            attentReq.StatusCode.Should().Be(HttpStatusCode.OK);
            Assert.True(rateReq.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, rateReq2.StatusCode);


            Assert.Equal("Good event 👨‍🦰🧑", response.Data.Feedback);
            Assert.Equal(4, response.Data.Rate);
        }


        [Fact, Priority(10)]
        public async Task A_Create_Rating_From_NonParticipant()
        {
            // Arrange
            var hostAuthResult = await AuthenticateAsync();
            var createEvent = await CreateSamplePostAsync();
            var guestAuthResult = await AuthenticateAsync();

            // Act
            var rating = new CreateRatingRequest
            {
                Feedback = "Good event 👨‍🦰🧑",
                PostId = createEvent.Id,
                UserId = hostAuthResult.UserId,
                Rate = 4
            };

            var rateReq = await TestClient.PostAsJsonAsync(ApiRoutes.Ratings.Create, rating);
            var response = await rateReq.Content.ReadFromJsonAsync<Response<CreateRatingResponse>>();

            // Assert
            Assert.NotEqual(HttpStatusCode.InternalServerError, rateReq.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, rateReq.StatusCode);
            Assert.Null(response.Data);
        }

// DELETE RATING TEST ------------------------------------------------------------------------------------------------------------------------------------------------------

       
        [Fact, Priority(20)]
        public async Task C_Delete_Unexistent_Rating()
        {
            // Arrage
            AuthenticateAdmin();


            // Act
            var deleteReq = await TestClient.DeleteAsync(ApiRoutes.Ratings.Delete.Replace("{ratingId}", "92345435"));

            // Assert
            Assert.NotEqual(HttpStatusCode.InternalServerError, deleteReq.StatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, deleteReq.StatusCode);
        }


        [Fact, Priority(20)]
        public async Task C_UnAuthorized_Delete_Rating()
        {
            // Arrage
            await AuthenticateAsync();


            // Act
            var deleteReq = await TestClient.DeleteAsync(ApiRoutes.Ratings.Delete.Replace("{ratingId}", _ratingId.ToString()));

            // Assert
            Assert.NotEqual(HttpStatusCode.InternalServerError, deleteReq.StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, deleteReq.StatusCode);
        }


        [Fact, Priority(25)]
        public async Task Z_Delete_Valid_Rating()
        {
            // Arrage
            AuthenticateAdmin();


            // Act
       //     await CreateValidSampleEvent();
            var deleteReq = await TestClient.DeleteAsync(ApiRoutes.Ratings.Delete.Replace("{ratingId}", _ratingId.ToString()));

            // Assert
            Assert.NotEqual(HttpStatusCode.InternalServerError, deleteReq.StatusCode);
            Assert.Equal(HttpStatusCode.NoContent, deleteReq.StatusCode);
        }

// GET ALL RATINGS----------------------------------------------------------------------------------------------------------------------------------------------------------
        
        [Fact, Priority(30)]
        public async Task Z_Get_All_Ratings_Valid()
        {
            // Arrange
            var host = await AuthenticateAsync();
            var createEvent = await CreateSamplePostAsync();


            // Act
            var rating = new CreateRatingRequest
            {
                Feedback = "Good event 👨‍🦰🧑",
                PostId = createEvent.Id,
                UserId = host.UserId,
                Rate = 4
            };
            var guest1AuthResult = await AuthenticateAsync(); 
            var attentReq1 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", createEvent.Id.ToString()), null);
            var rateReq1 = await TestClient.PostAsJsonAsync(ApiRoutes.Ratings.Create, rating);
            var response1 = await rateReq1.Content.ReadFromJsonAsync<Response<CreateRatingResponse>>();

            var guest2AuthResult = await AuthenticateAsync();
            var attentReq2 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", createEvent.Id.ToString()), null);
            var rateReq2 = await TestClient.PostAsJsonAsync(ApiRoutes.Ratings.Create, rating);
            var response2 = await rateReq2.Content.ReadFromJsonAsync<Response<CreateRatingResponse>>();

            var guest3AuthResult = await AuthenticateAsync();
            var attentReq3 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", createEvent.Id.ToString()), null);
            var rateReq3 = await TestClient.PostAsJsonAsync(ApiRoutes.Ratings.Create, rating);
            var response3 = await rateReq3.Content.ReadFromJsonAsync<Response<CreateRatingResponse>>();

            var guest4AuthResult = await AuthenticateAsync();
            var attentReq4 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", createEvent.Id.ToString()), null);
            var rateReq4 = await TestClient.PostAsJsonAsync(ApiRoutes.Ratings.Create, rating);
            var response4 = await rateReq4.Content.ReadFromJsonAsync<Response<CreateRatingResponse>>();

            var guest5AuthResult = await AuthenticateAsync();
            var attentReq5 = await TestClient.PostAsync(ApiRoutes.AttendedEvents.Attend.Replace("{postId}", createEvent.Id.ToString()), null);
            var rateReq5 = await TestClient.PostAsJsonAsync(ApiRoutes.Ratings.Create, rating);
            var response5 = await rateReq5.Content.ReadFromJsonAsync<Response<CreateRatingResponse>>();

            var ratingsResponse = await TestClient.GetAsync(ApiRoutes.Ratings.GetAll.Replace("{userId}", host.UserId));
            var fetchRatingsResp = await ratingsResponse.Content.ReadFromJsonAsync<Response<List<GetRating>>>();

            // Assert
            attentReq1.StatusCode.Should().Be(HttpStatusCode.OK);
            attentReq2.StatusCode.Should().Be(HttpStatusCode.OK);
            attentReq3.StatusCode.Should().Be(HttpStatusCode.OK);
            attentReq4.StatusCode.Should().Be(HttpStatusCode.OK);
            attentReq5.StatusCode.Should().Be(HttpStatusCode.OK);
            ratingsResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            Assert.NotNull(response1.Data);
            Assert.NotNull(response2.Data);
            Assert.NotNull(response3.Data);
            Assert.NotNull(response4.Data);
            Assert.NotNull(response5.Data);
            Assert.NotNull(fetchRatingsResp.Data);

            Assert.True(fetchRatingsResp.Data.Count == 5);
            Assert.True(fetchRatingsResp.Data.Select(r => r.Rate).Average() == 4);

        }

        [Fact, Priority(50)]
        public async Task Get_All_Ratings_When_None()
        {
            // Arrange
            var host = await AuthenticateAsync();

            // Act
            var ratingsResponse = await TestClient.GetAsync(ApiRoutes.Ratings.GetAll.Replace("{userId}", host.UserId));

            // Assert
            ratingsResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

    }
}
