using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.User;
using Bingo.Contracts.V1.Responses;
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
            Assert.Null(result.LastName);
        }


        [Fact, Priority(10)]
        public async Task B_Delete_User_Bad_Id()
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
    }
}
