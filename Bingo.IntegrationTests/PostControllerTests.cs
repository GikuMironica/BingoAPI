using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.Post;
using BingoAPI.Models;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Bingo.IntegrationTests
{
    public class PostControllerTests : PostIntegrationTest
    {
        [Fact]
        public async Task Get_PostWhen_Doesnt_Exist()
        {
            // Arrange
            await AuthenticateAsync();

            // Act
            var response = await TestClient.GetAsync(ApiRoutes.Posts.Get.Replace("{postId}", "dfsgge"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        
        [Fact]
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

        [Fact]
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


        [Fact]
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

        [Fact]
        public async Task Create_PostWith_Valid_Data()
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

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

    }

    
}
