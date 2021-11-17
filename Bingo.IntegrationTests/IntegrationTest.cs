using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.Identity;
using Bingo.Contracts.V1.Requests.User;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.Identity;
using BingoAPI;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Bingo.IntegrationTests
{

    public class IntegrationTest
    {
        protected readonly HttpClient TestClient;
        private readonly string _token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiJlYmU0ZjJjOC0wMzk0LTRhMTUtODc5ZC00ZDBhYTM5ZGE1MDMiLCJlbWFpbCI6ImFkbWluaXN0cmF0aW9uQGhvcGF1dC5jb20iLCJpZCI6IjY0NWZkYmNiLTc1MTItNDIwOC05ODI0LTBjMWM0NjBhZDFlNyIsInBvc3QuYWRkIjoidHJ1ZSIsInJvbGUiOlsiQWRtaW4iLCJVc2VyIiwiU3VwZXJBZG1pbiJdLCJuYmYiOjE2MzU1NTE2NjEsImV4cCI6MTYzNTU1MzQ2MSwiaWF0IjoxNjM1NTUxNjYxfQ.BimRkvbo1URDRuKHZ579ZviS5D7_6G_wJts4z1tpAIg";
        public IntegrationTest()
        {
            var appFactory = new WebApplicationFactory<Startup>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {

                  //      services.RemoveAll(typeof(DataContext));
                  //      services.AddDbContext<DataContext>(options =>
                  //      {
                  //          options.UseInMemoryDatabase("TestDB");
                  //      });
                    });
                });
            TestClient = appFactory.CreateClient();
        }

        protected async Task<AuthenticationResult> AuthenticateAsync()
        {
            var authResult = await GetJwtAsync();
            TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", authResult.JWT);

            var updateData = new UpdateUserRequest
            {
                FirstName = "Test",
                LastName = "User"
            };
            var updateUserRequest = await TestClient.PutAsJsonAsync(ApiRoutes.Users.Update.Replace("{userId}", authResult.UserId), updateData);
            if (!updateUserRequest.IsSuccessStatusCode)
                throw new NullReferenceException();

            return authResult;
        }

        protected void AuthenticateAdmin()
        {
            TestClient.DefaultRequestHeaders.Authorization =
               new AuthenticationHeaderValue("bearer", _token);
        }       

        private async Task<AuthenticationResult> GetJwtAsync()
        {
            var uniqueName = Guid.NewGuid().ToString()+ "test@integration.com";
            var password = "SomePass123";
            var request = new UserRegistrationRequest
            {
                Email = uniqueName,
                Password = password
            };
            var response = await TestClient.PostAsJsonAsync(ApiRoutes.Identity.Register, request);

        
            var loginRequest = new UserLoginRequest
            {
                Email = request.Email,
                Password = request.Password
            };

            var confirmEmail = new ConfirmEmailRequest
            {
                Email = loginRequest.Email
            };

            // set admin jwt token in order to confirm the email
            TestClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("bearer", _token);
            var result = await TestClient.PostAsJsonAsync(ApiRoutes.Identity.AdminConfirmEmail, confirmEmail);

            var loginResponse = await TestClient.PostAsJsonAsync(ApiRoutes.Identity.Login, loginRequest);

            var registrationResponse = await loginResponse.Content.ReadFromJsonAsync<AuthSuccessResponse>();
            var userId = await response.Content.ReadFromJsonAsync<Response<string>>();
            return new AuthenticationResult
            {
                JWT = registrationResponse.Token,
                UserId = userId.Data
            };
        }

        public void UpdateToken(string token)
        {
            TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
        }
    }
}
