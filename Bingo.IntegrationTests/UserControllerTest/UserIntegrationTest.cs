using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.User;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.User;
using Bingo.IntegrationTests.AnnouncementControllerTest;
using BingoAPI.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Bingo.IntegrationTests.UserControllerTest
{
    public class UserIntegrationTest : AnnouncementIntegrationTest
    {
        public async Task<UserResponse> UpdateUserAsync(UpdateUserRequest updateUser, string userId)
        {
            var response = await TestClient.PutAsJsonAsync(ApiRoutes.Users.Update.Replace("{userId}",userId), updateUser);
            var responseTxt = await response.Content.ReadAsStringAsync();
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                throw new NullReferenceException();
            }
            if (!response.IsSuccessStatusCode)
                return null;

            var data = await response.Content.ReadFromJsonAsync<Response<UserResponse>>();
            return data.Data;
        }
    }
}
