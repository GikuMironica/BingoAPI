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
        private readonly string _token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbmlzdHJhdGlvbkBob3BhdXQuY29tIiwianRpIjoiZTYwNWJiMDUtNzU0Zi00MWI1LWFlNzctMzI3YmQ3MDQzYTk1IiwiZW1haWwiOiJhZG1pbmlzdHJhdGlvbkBob3BhdXQuY29tIiwiaWQiOiJkNjFkNWJhMS00MWNhLTQ0ZjMtOTI3NC05YmUyN2JmZjE1MTIiLCJwb3N0LmFkZCI6InRydWUiLCJyb2xlIjpbIkFkbWluIiwiVXNlciIsIlN1cGVyQWRtaW4iXSwibmJmIjoxNjA1MTIwMDYxLCJleHAiOjE2MDUxMjE4NjEsImlhdCI6MTYwNTEyMDA2MX0.elkEvsD9ts7p2U5iGj5HMCFmoEQpLkPMZ6PRwnYB_58";
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
