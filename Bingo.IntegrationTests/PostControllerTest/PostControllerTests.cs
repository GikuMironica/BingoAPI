using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.Post;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.Post;
using BingoAPI.Models;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private readonly string _Longitude = "?UserLocation.Longitude={0}";
        private readonly string _Latitude = "&UserLocation.Latitude={0}";
        private readonly string _Radius = "&UserLocation.RadiusRange={0}";
        private readonly string _HouseParty = "&HouseParty=true";
        private readonly string _Club = "&Club=true";
        private readonly string _Bar = "&Bar=true";
        private readonly string _StreetParty = "&StreetParty=true";
        private readonly string _CarMeet = "&CarMeet=true";
        private readonly string _BikerMeet = "&BikerMeet=true";
        private readonly string _BicycleMeet = "&BicycleMeet=true";
        private readonly string _Marathon = "&Marathon=true";
        private readonly string _Other = "&Other=true";
        private readonly string _Tag = "&Tag={0}";
        private readonly string _Today = "&Today=true";


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
            Assert.Equal(9.23235, post.Data.Location.Longitude);
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
            Assert.Equal(9.23235, post.Data.Location.Longitude);
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
            Assert.Equal(9.23235, post.Data.Location.Longitude);
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
            Assert.Equal(9.13235, post.Data.Location.Longitude);
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
            Assert.Equal(9.992980, post.Data.Location.Longitude);
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
            Assert.Equal(9.992980, post.Data.Location.Longitude);
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

// GET ALL POSTS - FILTER TEST

        [Fact, Priority(1)]
        public async Task Get_AllPosts_No_Preferences()
        {
            // Arrange
            var host1 = await AuthenticateAsync();
            var post1 = await CreateSamplePostAsync("hello", 1, 9.89, 10);
            var host2 = await AuthenticateAsync();
            var post2 = await CreateSamplePostAsync("butwhysir", 2, 8.89, 5, 3600, 15000);
            var host3 = await AuthenticateAsync();
            var post3 = await CreateSamplePostAsync("wow", 1, 7.89, 5, 1800, 18000);
            var gues = await AuthenticateAsync();

            var URL = ApiRoutes.Posts.GetAll + _Longitude.Replace("{0}", "13.49") + _Latitude.Replace("{0}", "52.53") + _Radius.Replace("{0}", "15");

            // Act
            var getPostRequest = await TestClient.GetAsync(URL);
            var dataResponse = await getPostRequest.Content.ReadFromJsonAsync<Response<List<Posts>>>();

            // Assert
            getPostRequest.StatusCode.Should().Be(HttpStatusCode.OK);
            Assert.NotNull(dataResponse.Data);

            Assert.True(dataResponse.Data.Count() >= 3);
            var foundPosts = dataResponse.Data
                .Where(p => p.PostId == post1.Id
                         || p.PostId == post2.Id
                         || p.PostId == post3.Id);

            Assert.True(foundPosts.Count() == 3);            
        }


        [Fact, Priority(1)]
        public async Task Get_AllPosts_By_Tag()
        {
            // Arrange
            var host1 = await AuthenticateAsync();
            var post1 = await CreateSamplePostAsync("bustard", 1, 9.89, 10);
            var host2 = await AuthenticateAsync();
            var post2 = await CreateSamplePostAsync("yoyoyo", 2, 8.89, 5, 3600, 15000);
            var gues = await AuthenticateAsync();

            var URL = ApiRoutes.Posts.GetAll + _Longitude.Replace("{0}", "13.404") + _Latitude.Replace("{0}", "52.52") + _Radius.Replace("{0}", "15") + _Tag.Replace("{0}","bustard");

            // Act
            var getPostRequest = await TestClient.GetAsync(URL);
            var dataResponse = await getPostRequest.Content.ReadFromJsonAsync<Response<List<Posts>>>();


            // Assert
            getPostRequest.StatusCode.Should().Be(HttpStatusCode.OK);
            Assert.NotNull(dataResponse.Data);
            Assert.True(dataResponse.Data.Count() >= 1);
            var foundPosts = dataResponse.Data
                .Where(p => p.PostId == post1.Id);
            Assert.True(foundPosts.Count() == 1);
        }


        [Fact, Priority(1)]
        public async Task Get_AllPosts_By_Today()
        {
            // Arrange
            var host1 = await AuthenticateAsync();
            var post1 = await CreateSamplePostAsync("Dickbag", 1, 5.89, 15);
            var host2 = await AuthenticateAsync();
            var post2 = await CreateSamplePostAsync("nutbag", 2, 6.89, 5, 30000, 46000);
            var gues = await AuthenticateAsync();

            var URL = ApiRoutes.Posts.GetAll + _Longitude.Replace("{0}", "13.412") + _Latitude.Replace("{0}", "52.534") + _Radius.Replace("{0}", "15") + _Today.Replace("{0}", "true");

            // Act
            var getPostRequest = await TestClient.GetAsync(URL);
            var dataResponse = await getPostRequest.Content.ReadFromJsonAsync<Response<List<Posts>>>();


            // Assert
            getPostRequest.StatusCode.Should().Be(HttpStatusCode.OK);
            Assert.NotNull(dataResponse.Data);
            Assert.True(dataResponse.Data.Count() >= 2);
            var foundPosts = dataResponse.Data
                .Where(p => p.PostId == post1.Id
                         || p.PostId == post2.Id);
            Assert.True(foundPosts.Count() == 2);
        }


        [Fact, Priority(1)]
        public async Task Get_AllPosts_By_Today_Load_Test()
        {
            // Arrange
            var host1 = await AuthenticateAsync();
            var post1 = await CreateSamplePostAsync("Dickbag", 1, 5.89, 15);
            var host2 = await AuthenticateAsync();
            var post2 = await CreateSamplePostAsync("nutbag", 2, 6.89, 5, 30000, 46000);
            var gues = await AuthenticateAsync();

            var URL = ApiRoutes.Posts.GetAll + _Longitude.Replace("{0}", "13.412") + _Latitude.Replace("{0}", "52.534") + _Radius.Replace("{0}", "15") + _Today.Replace("{0}", "true");

            // Act
            var getPostRequest = await TestClient.GetAsync(URL);
            for(int i = 0; i<100; i++)
            {
                getPostRequest = await TestClient.GetAsync(URL);
                getPostRequest.StatusCode.Should().Be(HttpStatusCode.OK);
            }
            var dataResponse = await getPostRequest.Content.ReadFromJsonAsync<Response<List<Posts>>>();


            // Assert
            getPostRequest.StatusCode.Should().Be(HttpStatusCode.OK);
            Assert.NotNull(dataResponse.Data);
            Assert.True(dataResponse.Data.Count() >= 2);
            var foundPosts = dataResponse.Data
                .Where(p => p.PostId == post1.Id
                         || p.PostId == post2.Id);
            Assert.True(foundPosts.Count() == 2);
        }



        [Fact, Priority(1)]
        public async Task Get_AllPosts_By_Few_Types()
        {
            // Arrange
            var host1 = await AuthenticateAsync();
            var post1 = await CreateSamplePostAsync("Nutbag", 1, 5.89, 15);
            var host2 = await AuthenticateAsync();
            var post3 = await CreateSamplePostAsync("Nutbag", 2, 5.89, 15);
            var host3 = await AuthenticateAsync();
            var post4 = await CreateSamplePostAsync("Nutbag", 3, 5.89, 15);
            var host4 = await AuthenticateAsync();
            var post2 = await CreateSamplePostAsync("Lollo", 4, 6.89, 5, 33400, 45000);
            var host5 = await AuthenticateAsync();
            var post5 = await CreateSamplePostAsync("Lgeol", 5, 6.89, 5, 31500, 44000);
            var host6 = await AuthenticateAsync();
            var post6 = await CreateSamplePostAsync("Logegl", 6, 6.89, 5, 30430, 49000);
            var host7 = await AuthenticateAsync();
            var post7 = await CreateSamplePostAsync("Lofdgl", 7, 6.89, 5, 29000, 38000);
            var gues = await AuthenticateAsync();

            var URL = ApiRoutes.Posts.GetAll + _Longitude.Replace("{0}", "13.412") + _Latitude.Replace("{0}", "52.534") 
                + _Radius.Replace("{0}", "15") + _HouseParty+_Bar+_Club;

            // Act
            var getPostRequest = await TestClient.GetAsync(URL);            
            var dataResponse = await getPostRequest.Content.ReadFromJsonAsync<Response<List<Posts>>>();


            // Assert
            getPostRequest.StatusCode.Should().Be(HttpStatusCode.OK);
            Assert.NotNull(dataResponse.Data);
            Assert.True(dataResponse.Data.Count() >= 3);
            var foundPosts = dataResponse.Data
                .Where(p => p.PostId == post1.Id
                         || p.PostId == post4.Id
                         || p.PostId == post3.Id);
            Assert.True(foundPosts.Count() == 3);
        }


        [Fact, Priority(1)]
        public async Task Get_AllPosts_By_Types_And_Today()
        {
            // Arrange
            var host1 = await AuthenticateAsync();
            var post1 = await CreateSamplePostAsync("Lollo", 4, 6.89, 5, 33400, 45000);
            var host2 = await AuthenticateAsync();
            var post2 = await CreateSamplePostAsync("Lgeol", 5, 6.89, 5, 31500, 44000);
            var host3 = await AuthenticateAsync();
            var post3 = await CreateSamplePostAsync("Logegl", 6, 6.89, 5, 30430, 49000);
            var host4 = await AuthenticateAsync();
            var post4 = await CreateSamplePostAsync("Lofdgl", 7, 6.89, 5, 60000, 68000);
            var gues = await AuthenticateAsync();

            var URL = ApiRoutes.Posts.GetAll + _Longitude.Replace("{0}", "13.412") + _Latitude.Replace("{0}", "52.534")
                + _Radius.Replace("{0}", "15") + _BikerMeet + _BicycleMeet + _CarMeet + _StreetParty + _Today;

            // Act
            var getPostRequest = await TestClient.GetAsync(URL);
            var dataResponse = await getPostRequest.Content.ReadFromJsonAsync<Response<List<Posts>>>();


            // Assert
            getPostRequest.StatusCode.Should().Be(HttpStatusCode.OK);
            Assert.NotNull(dataResponse.Data);
            Assert.True(dataResponse.Data.Count() >= 3);
            var foundPosts = dataResponse.Data
                .Where(p => p.PostId == post1.Id
                         || p.PostId == post2.Id
                         || p.PostId == post3.Id);
            Assert.True(foundPosts.Count() == 3);
        }


        [Fact, Priority(1)]
        public async Task Get_AllPosts_By_Types_And_Tag()
        {
            // Arrange
            var host1 = await AuthenticateAsync();
            var post1 = await CreateSamplePostAsync("NoNo", 9, 6.89, 5, 33400, 45000);
            var host2 = await AuthenticateAsync();
            var post2 = await CreateSamplePostAsync("Lgeol", 5, 6.89, 5, 31500, 44000);
            var host3 = await AuthenticateAsync();
            var post3 = await CreateSamplePostAsync("NoNo", 9, 6.89, 5, 30430, 49000);
            var host4 = await AuthenticateAsync();
            var post4 = await CreateSamplePostAsync("Lofdgl", 9, 6.89, 5, 60000, 68000);
            var gues = await AuthenticateAsync();

            var URL = ApiRoutes.Posts.GetAll + _Longitude.Replace("{0}", "13.412") + _Latitude.Replace("{0}", "52.534")
                + _Radius.Replace("{0}", "15") + _BikerMeet + _BicycleMeet + _CarMeet + _StreetParty + _Other + _Tag.Replace("{0}", "nono");

            // Act
            var getPostRequest = await TestClient.GetAsync(URL);
            var dataResponse = await getPostRequest.Content.ReadFromJsonAsync<Response<List<Posts>>>();


            // Assert
            getPostRequest.StatusCode.Should().Be(HttpStatusCode.OK);
            Assert.NotNull(dataResponse.Data);
            Assert.True(dataResponse.Data.Count() >= 2);
            var foundPosts = dataResponse.Data
                .Where(p => p.PostId == post1.Id
                         || p.PostId == post3.Id);
            Assert.True(foundPosts.Count() == 2);
        }


        [Fact, Priority(1)]
        public async Task Get_AllPosts_By_Types_And_Tag_And_Today()
        {
            // Arrange
            var host1 = await AuthenticateAsync();
            var post1 = await CreateSamplePostAsync("MF", 9, 6.89, 5, 70000, 75000);
            var host2 = await AuthenticateAsync();
            var post2 = await CreateSamplePostAsync("Lgeol", 5, 6.89, 5, 31500, 44000);
            var host3 = await AuthenticateAsync();
            var post3 = await CreateSamplePostAsync("MF", 9, 6.89, 5, 30430, 49000);
            var host4 = await AuthenticateAsync();
            var post4 = await CreateSamplePostAsync("Lofdgl", 9, 6.89, 5, 60000, 68000);
            var host5 = await AuthenticateAsync();
            var post5 = await CreateSamplePostAsync("Lofdgl", 1, 6.89, 5, 60000, 68000);
            var host6 = await AuthenticateAsync();
            var post6 = await CreateSamplePostAsync("Lofdgl", 2, 6.89, 5, 60000, 68000);
            var gues = await AuthenticateAsync();

            var URL = ApiRoutes.Posts.GetAll + _Longitude.Replace("{0}", "13.412") + _Latitude.Replace("{0}", "52.534")
                + _Radius.Replace("{0}", "15") + _BikerMeet + _BicycleMeet + _CarMeet + _StreetParty + _Other +_Today +_Tag.Replace("{0}", "MF");

            // Act
            var getPostRequest = await TestClient.GetAsync(URL);
            var dataResponse = await getPostRequest.Content.ReadFromJsonAsync<Response<List<Posts>>>();


            // Assert
            getPostRequest.StatusCode.Should().Be(HttpStatusCode.OK);
            Assert.NotNull(dataResponse.Data);
            Assert.True(dataResponse.Data.Count() >= 1);

            var foundPosts = dataResponse.Data
                .Where(p => p.PostId == post3.Id);
            var shouldNotBeThere = dataResponse.Data
                .Where(p => p.PostId == post1.Id
                         || p.PostId == post2.Id
                         || p.PostId == post4.Id
                         || p.PostId == post5.Id
                         || p.PostId == post6.Id);

            Assert.Empty(shouldNotBeThere);
            Assert.True(foundPosts.Count() == 1);
        }


        [Fact, Priority(1)]
        public async Task Get_AllPosts_When_None()
        {
            // Arrange
            var host = await AuthenticateAsync();

            var URL = ApiRoutes.Posts.GetAll + _Longitude.Replace("{0}", "99.999") + _Latitude.Replace("{0}", "90.000")
                + _Radius.Replace("{0}", "15");

            // Act
            var getPostRequest = await TestClient.GetAsync(URL);

            // Assert
            getPostRequest.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }


        [Fact, Priority(1)]
        public async Task Get_AllPosts_By_Types_When_None()
        {
            // Arrange
            var host1 = await AuthenticateAsync();
            var post1 = await CreateSamplePostAsync("MF", 1, 6.89, 5, 70000, 75000);
            var host2 = await AuthenticateAsync();
            var post2 = await CreateSamplePostAsync("Lgeol", 2, 6.89, 5, 31500, 44000);
            var host3 = await AuthenticateAsync();
            var post3 = await CreateSamplePostAsync("MF", 3, 6.89, 5, 30430, 49000);
            var gues = await AuthenticateAsync();

            var URL = ApiRoutes.Posts.GetAll + _Longitude.Replace("{0}", "99.912") + _Latitude.Replace("{0}", "88.888")
                + _Radius.Replace("{0}", "15") + _Other;

            // Act
            var getPostRequest = await TestClient.GetAsync(URL);

            // Assert
            getPostRequest.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }


        [Fact, Priority(1)]
        public async Task Get_AllPosts_By_Today_When_None()
        {
            // Arrange
            var host1 = await AuthenticateAsync();
            var post1 = await CreateSamplePostAsync("MF", 1, 6.89, 5, 70000, 75000, -170, -50);
            var host2 = await AuthenticateAsync();
            var post2 = await CreateSamplePostAsync("Lgeol", 2, 6.89, 5, 75999, 85000, - 170, -50);
            var host3 = await AuthenticateAsync();
            var post3 = await CreateSamplePostAsync("MF", 3, 6.89, 5, 87500, 91000, - 170, -50);
            var gues = await AuthenticateAsync();

            var URL = ApiRoutes.Posts.GetAll + _Longitude.Replace("{0}", "-170.0") + _Latitude.Replace("{0}", "-50")
                + _Radius.Replace("{0}", "15") + _Today;

            // Act
            var getPostRequest = await TestClient.GetAsync(URL);

            // Assert
            getPostRequest.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }


        [Fact, Priority(1)]
        public async Task Get_AllPosts_By_Tag_When_None()
        {
            // Arrange
            var host1 = await AuthenticateAsync();
            var post1 = await CreateSamplePostAsync("MF", 1, 6.89, 5, 70000, 75000);
            var host2 = await AuthenticateAsync();
            var post2 = await CreateSamplePostAsync("Lgeol", 2, 6.89, 5, 31500, 44000);
            var host3 = await AuthenticateAsync();
            var post3 = await CreateSamplePostAsync("MF", 3, 6.89, 5, 30430, 49000);
            var gues = await AuthenticateAsync();

            var URL = ApiRoutes.Posts.GetAll + _Longitude.Replace("{0}", "13.404954") + _Latitude.Replace("{0}", "52.520008")
                + _Radius.Replace("{0}", "15") + _Tag.Replace("{0}", "32rAFh$&%*#fasfg2ауцац");

            // Act
            var getPostRequest = await TestClient.GetAsync(URL);


            // Assert
            getPostRequest.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

// GET ALL MY ACTIVE EVENTS TEST ----------------------------------------------------------------------------------------------------------------------------------------------------

        [Fact, Priority(1)]
        public async Task Get_All_MyActive_When_Any()
        {
            // Arrange
            var host = await AuthenticateAsync();
            var post1 = await CreateSamplePostAsync("123", 1, 6.89, 5, 70000, 75000);

            
            // Act
            var getPostRequest = await TestClient.GetAsync(ApiRoutes.Posts.GetAllActive);
            var dataResponse = await getPostRequest.Content.ReadFromJsonAsync<PagedResponse<Posts>>();


            // Assert
            getPostRequest.StatusCode.Should().Be(HttpStatusCode.OK);
            Assert.NotNull(dataResponse.Data);
            Assert.Single(dataResponse.Data);
        }

        [Fact, Priority(1)]
        public async Task Get_All_MyActive_When_None()
        {
            // Arrange
            var host = await AuthenticateAsync();
            var post1 = await CreateSamplePostAsync("123", 1, 6.89, 5, 70000, 75000);
            AuthenticateAdmin();
            var disable = new DisablePostRequest
            {
                Id = post1.Id
            };

            // Act
            var disableRequest = await TestClient.PutAsJsonAsync(ApiRoutes.Posts.DisablePost, disable);

            UpdateToken(host.JWT);
            var getPostRequest = await TestClient.GetAsync(ApiRoutes.Posts.GetAllActive);

            // Assert
            getPostRequest.StatusCode.Should().Be(HttpStatusCode.NoContent);
            disableRequest.StatusCode.Should().Be(HttpStatusCode.OK);
        }


        [Fact, Priority(1)]
        public async Task Get_All_MyInActive_When_Any()
        {
            // Arrange
            var host = await AuthenticateAsync();
            var post1 = await CreateSamplePostAsync("123", 1, 6.89, 5, 70000, 75000);
            AuthenticateAdmin();
            var disable = new DisablePostRequest
            {
                Id = post1.Id
            };

            // Act
            var disableRequest = await TestClient.PutAsJsonAsync(ApiRoutes.Posts.DisablePost, disable);

            UpdateToken(host.JWT);
            var getPostRequest = await TestClient.GetAsync(ApiRoutes.Posts.GetAllInactive);
            var data = await getPostRequest.Content.ReadFromJsonAsync<PagedResponse<Posts>>();

            // Assert
            getPostRequest.StatusCode.Should().Be(HttpStatusCode.OK);
            disableRequest.StatusCode.Should().Be(HttpStatusCode.OK);
            Assert.Single(data.Data);
        }


        [Fact, Priority(1)]
        public async Task Get_All_MyInActive_When_None()
        {
            // Arrange
            var host = await AuthenticateAsync();
            var post1 = await CreateSamplePostAsync("123", 1, 6.89, 5, 70000, 75000);
           
            // Act            
            var getPostRequest = await TestClient.GetAsync(ApiRoutes.Posts.GetAllInactive);

            // Assert
            getPostRequest.StatusCode.Should().Be(HttpStatusCode.NoContent);
        }


        public async Task<CreatePostResponse> CreateSamplePostAsync(string tag, int type=1, double price=9.99, int slots=10, int startTime = 10000, 
                                                                    int endTime = 12000, double longit= 0, double lat = 0)
        {
            var createdPost = new CreatePostRequest
            {
                EventTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + startTime,
                EndTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + endTime,
                UserLocation = new UserCompleteLocation
                {
                    Latitude = 52.520008,
                    Longitude = 13.404954,
                    Address = "Street",
                    City = "UlmTest",
                    Country = "mars",
                    Region = "BW"
                },
                Event = new ContainedEvent
                {
                    Title = "Test Event for announcement",
                    Description = "Test post for announcement",
                    Requirements = "None",
                    EventType = type,
                    EntrancePrice = price,
                    Slots = slots
                },
                Tags = new List<string> { Guid.NewGuid().ToString(), tag }
            };
            if(longit != 0 && lat != 0)
            {
                createdPost.UserLocation.Latitude = lat;
                createdPost.UserLocation.Longitude = longit;
            }

            var result = await CreatePostAsync(createdPost);
            return result.Data;
        }

    }


}
