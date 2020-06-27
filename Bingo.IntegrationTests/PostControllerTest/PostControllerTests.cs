using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.Post;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.Post;
using BingoAPI.Models;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Priority;
using Xunit.Sdk;

namespace Bingo.IntegrationTests.PostControllerTest
{
    [TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
    public class PostControllerTests : PostIntegrationTest
    {
        private static int _housePartyId;
        private static int _streetPatyId;
        private static string _deleted;

       
        /* Create Test ----------------------------------------------------------------------------------------------------------------------------------------*/

        [Fact, Priority(-10)]
        public async Task Create_PostWith_Invalid_Location()
        {
            // Arrange
            await AuthenticateAsync();
            var createdPost = new CreatePostRequest
            {
                EventTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                EndTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600,
                UserLocation = new UserCompleteLocation
                {
                    Latitude = 485.143,
                    Longitude = 923.23235
                },
                Event = new ContainedEvent
                {
                    Title = "Wahahah",
                    Description = "Test lolmanigaaadsasd",
                    Requirements = "None",
                    Slots = 10,
                    EntrancePrice = 400,
                    EventType = 1
                }
            };

            // Act
            var result = await CreatePostAsync(createdPost);

            // Assert
            Assert.Null(result);
        }

        [Fact, Priority(-10)]
        public async Task Create_PostWith_Invalid_Times()
        {
            // Arrange
            await AuthenticateAsync();
            var createdPost = new CreatePostRequest
            {
                EventTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 1801,
                EndTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 1200,
                UserLocation = new UserCompleteLocation
                {
                    Latitude = 48.2342,
                    Longitude = 9.23235
                },
                Event = new ContainedEvent
                {
                    Title = "Wahahah",
                    Description = "Test lolmanigaaadsasd",
                    Requirements = "None",
                    Slots = 10,
                    EntrancePrice = 400,
                    EventType = 1
                }
            };

            // Act
            var result = await CreatePostAsync(createdPost);

            // Assert
            Assert.Null(result);
        }


        [Fact, Priority(-10)]
        public async Task Create_PostWith_Invalid_Data_2IdenticTags()
        {
            // Arrange
            await AuthenticateAsync();
            var createdPost = new CreatePostRequest
            {
                EventTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 600,
                EndTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 301,
                UserLocation = new UserCompleteLocation
                {
                    Latitude = 48.143,
                    Longitude = 9.23235
                },
                Event = new ContainedEvent
                {
                    Title = "Wahahah",
                    Description = "WAhahahaha lolmanigaaadsasd",
                    Requirements = "None",
                    Slots = 10,
                    EntrancePrice = 400,
                    EventType = 7
                },
                Tags = new List<string> { "TestTag1","TestTag1"}
                
            };

            // Act 
            var result = await CreatePostAsync(createdPost);
            //var response = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", result.Data.Id.ToString()));

            // Assert
            Assert.Null(result);
        }

        [Fact, Priority(0)]
        public async Task Create_HousePartyPostWith_Valid_Data()
        {
            // Arrange
            await AuthenticateAsync();
            var createdPost = new CreatePostRequest
            {
                EventTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                EndTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 36252,
                UserLocation = new UserCompleteLocation
                {
                    Latitude = 48.143,
                    Longitude = 9.23235,
                    Address = "My place",
                    City = "UlmTest",
                    Country = "mars",
                    EntityName = "HopAutHQ",
                    Region = "BW"
                },
                Event = new ContainedEvent
                {
                    Title = "Wahahah",
                    Description = "Test lolmanigaaadsasd",
                    Requirements = "None",
                    Slots = 10,
                    EntrancePrice = 400,
                    EventType = 1
                },
                Tags = new List<string> { Guid.NewGuid().ToString(), "RepeatedTag"}
            };

            // Act
            var result = await CreatePostAsync(createdPost);
            var response = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", result.Data.Id.ToString()));
            _housePartyId = result.Data.Id;

            // Assert
            var post = await response.Content.ReadFromJsonAsync<Response<PostResponse>>();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            Assert.NotNull(post.Data.Location);
            Assert.NotNull(post.Data.Event);
            Assert.Equal(1, post.Data.Event.EventType);
            Assert.Equal(400, post.Data.Event.EntrancePrice);
            Assert.Equal(10, post.Data.Event.Slots);
            Assert.Equal("BW", post.Data.Location.Region);
            Assert.Equal(0, post.Data.HostRating);
            Assert.NotEqual(0, post.Data.VoucherDataId);
        }


        [Fact, Priority(1)]
        public async Task Create_StreetPartyPost_With_Valid_Data()
        {
            // Arrange
            await AuthenticateAsync();
            var createdPost = new CreatePostRequest
            {
                EventTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                EndTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600,
                UserLocation = new UserCompleteLocation
                {
                    Latitude = 48.256,
                    Longitude = 9.13235,
                    Address = "Street",
                    City = "UlmTest",
                    Country = "mars",
                    Region = "BW"
                },
                Event = new ContainedEvent
                {
                    Title = "Azazazazazazazazazaz",
                    Description = "Test lolmanigaaadsasd",
                    Requirements = "None",
                    Slots = 5,
                    EntrancePrice = 9.99,
                    EventType = 7                    
                },
                Tags = new List<string> { Guid.NewGuid().ToString(), "RepeatedTag" }
            };


            // Act
            var result = await CreatePostAsync(createdPost);
            var response = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", result.Data.Id.ToString()));
            _streetPatyId = result.Data.Id;


            // Assert
            var post = await response.Content.ReadFromJsonAsync<Response<PostResponse>>();
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            Assert.NotNull(post.Data);
            Assert.Equal(7, post.Data.Event.EventType);
            Assert.Equal(0, post.Data.Event.EntrancePrice);
            Assert.Equal(0, post.Data.Event.Slots);
            Assert.Null(post.Data.Location.EntityName);

        }


        /* Update Test ----------------------------------------------------------------------------------------------------------------------------------------*/


        [Fact, Priority(10)]              
        public async Task Update_HouseParty_WithInvalidLocation()
        {
            // Arrange
            AuthenticateAdmin();
            var updatePost = new UpdatePostRequest
            {
                EventTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 10,
                EndTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 3600,
                UserLocation = new UpdatedCompleteLocation
                {
                    Latitude = 90.01,
                    Longitude = -180.00001,
                    Address = "Street",
                    City = "UlmTest2",
                    Country = "mars"
                },
                Event = new Contracts.V1.Requests.Post.UpdatedEvent
                {
                    Title = "Azazazazazazazazazaz",
                    Description = "Test lolmanigaaadsasd",
                    Requirements = "None",
                    Slots = 5,
                    EntrancePrice = 9.99,
                },
                TagNames = new List<string> { "UpdatedTag" }
            };

            // Act
            var result = await UpdatePostAsync(updatePost, _housePartyId);
            Assert.False(result);
            var response = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", _housePartyId.ToString()));
            var post = await response.Content.ReadFromJsonAsync<Response<PostResponse>>();

            // Assert
            Assert.Equal(48.143, post.Data.Location.Latitude);
            Assert.Equal(9.23235, post.Data.Location.Logitude);
            Assert.Equal("UlmTest", post.Data.Location.City);
            Assert.Equal("My place", post.Data.Location.Address);
            Assert.Equal("HopAutHQ", post.Data.Location.EntityName);
            Assert.Equal("BW", post.Data.Location.Region);
            Assert.Equal(10, post.Data.Event.Slots);
            Assert.Equal(400, post.Data.Event.EntrancePrice);
            Assert.Equal(1, post.Data.Event.EventType);
            Assert.Equal("Test lolmanigaaadsasd", post.Data.Event.Description);
           
        }


        [Fact, Priority(10)]
        public async Task Update_HouseParty_WithInvalidTime()
        {
            // Arrange
            AuthenticateAdmin();
            var updatePost = new UpdatePostRequest
            {   
                EventTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 900,
                EndTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()+ 500,
                UserLocation = new UpdatedCompleteLocation
                {
                    Latitude = 90.0,
                    Longitude = -180.0000,
                    Address = "Street",
                    City = "UlmTest2",
                    Country = "mars"
                },
                Event = new Contracts.V1.Requests.Post.UpdatedEvent
                {
                    Title = "Azazazazazazazazazaz",
                    Description = "Test lolmanigaaadsasd",
                    Requirements = "None",
                    Slots = 5,
                    EntrancePrice = 9.99,
                },
                TagNames = new List<string> { "UpdatedTag" }
            };

            // Act
            var result = await UpdatePostAsync(updatePost, _housePartyId);
            Assert.False(result);
            var response = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", _housePartyId.ToString()));
            var post = await response.Content.ReadFromJsonAsync<Response<PostResponse>>();

            // Assert
            Assert.Equal(48.143, post.Data.Location.Latitude);
            Assert.Equal(9.23235, post.Data.Location.Logitude);
            Assert.Equal("UlmTest", post.Data.Location.City);
            Assert.Equal("My place", post.Data.Location.Address);
            Assert.Equal("HopAutHQ", post.Data.Location.EntityName);
            Assert.Equal("BW", post.Data.Location.Region);
            Assert.Equal(10, post.Data.Event.Slots);
            Assert.Equal(400, post.Data.Event.EntrancePrice);
            Assert.Equal(1, post.Data.Event.EventType);
            Assert.Equal("Test lolmanigaaadsasd", post.Data.Event.Description);
        }


        [Fact, Priority(10)]
        public async Task Update_HouseParty_WithInvalidEventData()
        {
            // Arrange
            AuthenticateAdmin();
            var updatePost = new UpdatePostRequest
            {
                
            };

            // Act
            var result = await UpdatePostAsync(updatePost, _housePartyId);
            Assert.False(result);
            var response = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", _housePartyId.ToString()));
            var post = await response.Content.ReadFromJsonAsync<Response<PostResponse>>();

            // Assert
            Assert.Equal(48.143, post.Data.Location.Latitude);
            Assert.Equal(9.23235, post.Data.Location.Logitude);
            Assert.Equal("UlmTest", post.Data.Location.City);
            Assert.Equal("My place", post.Data.Location.Address);
            Assert.Equal("HopAutHQ", post.Data.Location.EntityName);
            Assert.Equal("BW", post.Data.Location.Region);
            Assert.Equal(10, post.Data.Event.Slots);
            Assert.Equal(400, post.Data.Event.EntrancePrice);
            Assert.Equal(1, post.Data.Event.EventType);
            Assert.Equal("Test lolmanigaaadsasd", post.Data.Event.Description);
        }


        [Fact, Priority(10)]
        public async Task Update_StreetParty_WithInvalidEventData()
        {
            // Arrange
            AuthenticateAdmin();
            var updatePost = new UpdatePostRequest
            {
                EventTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                EndTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 5000,
                UserLocation = new UpdatedCompleteLocation
                {
                    Latitude = 90.0,
                    Longitude = -180.0000,
                    Address = "Street",
                    City = "UlmTest2",
                    Country = "mars"
                },
                Event = new Contracts.V1.Requests.Post.UpdatedEvent
                {
                    Title = "Azazazazazazazazazaz",
                    Description = "Test lolmanigaaadsasd",
                    Requirements = "AHAH",
                    Slots = 50,
                    EntrancePrice = 17.9                    
                },
                TagNames = new List<string> { "UpdatedTag","UpdatedTag" }
            };

            // Act
            var result = await UpdatePostAsync(updatePost, _streetPatyId);
            Assert.False(result);
            var response = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", _streetPatyId.ToString()));
            var post = await response.Content.ReadFromJsonAsync<Response<PostResponse>>();

            // Assert
            Assert.Equal(48.256, post.Data.Location.Latitude);
            Assert.Equal(9.13235, post.Data.Location.Logitude);
            Assert.Equal("UlmTest", post.Data.Location.City);
            Assert.Equal("Street", post.Data.Location.Address);
            Assert.Equal("BW", post.Data.Location.Region);
            Assert.Equal(0, post.Data.Event.Slots);
            Assert.Equal(0, post.Data.Event.EntrancePrice);
            Assert.Equal(7, post.Data.Event.EventType);
            Assert.Equal("Test lolmanigaaadsasd", post.Data.Event.Description);
            Assert.Equal("None", post.Data.Event.Requirements);
        }

        [Fact, Priority(15)]
        public async Task Update_HouseParty_WithValidData()
        {
            // Arrange
            AuthenticateAdmin();
            var updatePost = new UpdatePostRequest
            {
                EventTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                EndTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 5000,
                UserLocation = new UpdatedCompleteLocation
                {
                    Latitude = 48.124,
                    Longitude = 9.992980
                },
                Event = new Contracts.V1.Requests.Post.UpdatedEvent
                {
                    Title = "Valid House Party",
                    Description = "TestTestTestTest",
                    Requirements = "AHAH",
                    Slots = 50,
                    EntrancePrice = 17.9
                },
                TagNames = new List<string> { "ValidHausTag" }
            };

            // Act
            var result = await UpdatePostAsync(updatePost, _housePartyId);
            Assert.True(result);
            var response = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", _housePartyId.ToString()));
            var post = await response.Content.ReadFromJsonAsync<Response<PostResponse>>();

            // Assert
            Assert.Equal(48.124, post.Data.Location.Latitude);
            Assert.Equal(9.992980, post.Data.Location.Logitude);
            Assert.Equal("UlmTest", post.Data.Location.City);
            Assert.Equal("My place", post.Data.Location.Address);
            Assert.Equal("BW", post.Data.Location.Region);
            Assert.Equal(50, post.Data.Event.Slots);
            Assert.Equal(17.9, post.Data.Event.EntrancePrice);
            Assert.Equal(1, post.Data.Event.EventType);
            Assert.Equal("TestTestTestTest", post.Data.Event.Description);
            Assert.Equal("Valid House Party", post.Data.Event.Title);
            Assert.Equal("AHAH", post.Data.Event.Requirements);
            Assert.Equal("mars", post.Data.Location.Country);
        }

        [Fact, Priority(15)]
        public async Task Update_StreetParty_WithValidData()
        {
            // Arrange
            AuthenticateAdmin();
            var updatePost = new UpdatePostRequest
            {
                EventTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                EndTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 5000,
                UserLocation = new UpdatedCompleteLocation
                {
                    Latitude = 48.124,
                    Longitude = 9.992980
                },
                Event = new Contracts.V1.Requests.Post.UpdatedEvent
                {
                    Title = "Valid House Party"
                },
                TagNames = new List<string> { "ValidHausTag" }
            };
            

            // Act
            var result = await UpdatePostAsync(updatePost, _streetPatyId);
            Assert.True(result);
            var response = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", _streetPatyId.ToString()));
            var post = await response.Content.ReadFromJsonAsync<Response<PostResponse>>();

            // Assert
            Assert.Equal(48.124, post.Data.Location.Latitude);
            Assert.Equal(9.992980, post.Data.Location.Logitude);
            Assert.Equal("UlmTest", post.Data.Location.City);
            Assert.Equal("Street", post.Data.Location.Address);
            Assert.Equal("BW", post.Data.Location.Region);
            Assert.Equal(0, post.Data.Event.Slots);
            Assert.Equal(0, post.Data.Event.EntrancePrice);
            Assert.Equal(7, post.Data.Event.EventType);
            Assert.Equal("Test lolmanigaaadsasd", post.Data.Event.Description);
            Assert.Equal("Valid House Party", post.Data.Event.Title);
            Assert.Equal("None", post.Data.Event.Requirements);
            Assert.Equal("mars", post.Data.Location.Country);
        }


        // GET POSTS TEST ------------------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact, Priority(20)]
        public async Task Get_PostWhen_Doesnt_Exist()
        {
            // Arrange
            await AuthenticateAsync();

            // Act
            var response = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", "dfsgge"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact, Priority(20)]
        public async Task Get_Post_WithHouseP_When_Exists()
        {
            // Arrange
            await AuthenticateAsync();

            // Act
            var response = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", _housePartyId.ToString()));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var post = await response.Content.ReadFromJsonAsync<Response<PostResponse>>();
            var postData = post.Data;
            Assert.NotNull(post);
            Assert.Equal(1, postData.ActiveFlag);
            Assert.NotEqual(0, postData.EndTime);
            Assert.NotNull(postData.Event);
            Assert.NotNull(postData.Location);

        }


        [Fact, Priority(20)]
        public async Task Get_Post_WithStreetP_When_Exists()
        {
            // Arrange
            await AuthenticateAsync();

            // Act
            var response = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", _housePartyId.ToString()));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var post = await response.Content.ReadFromJsonAsync<Response<PostResponse>>();
            var postData = post.Data;
            Assert.NotNull(post);
            Assert.Equal(1, postData.ActiveFlag);
            Assert.NotEqual(0, postData.EndTime);
            Assert.NotNull(postData.Event);
            Assert.NotNull(postData.Location);

        }

// DELETE POST TEST -------------------------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact, Priority(25)]
        public async Task Delete_Post_When_Exist()
        {
            // Arrange
            AuthenticateAdmin();

            // Act
            _deleted = new Random().Next(_housePartyId, _streetPatyId+1).ToString();
            var deleteResponse = await TestClient.DeleteAsync(ApiRoutes.Posts.Delete.Replace("{postId}", _deleted));
            var getResponse = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", _deleted));

            // Assert
            deleteResponse.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        }

        [Fact, Priority(50)]
        public async Task ZZ_Delete_Post_When_Doesnt_Exist()
        {
            // Arrange
           AuthenticateAdmin();

            // Act
            var deleteResponse = await TestClient.DeleteAsync(ApiRoutes.Posts.Delete.Replace("{postId}", _deleted));
            var getResponse = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", _deleted));

            // Assert
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        }


    }


}
