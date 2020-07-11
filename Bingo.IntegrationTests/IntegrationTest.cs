using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.Identity;
using Bingo.Contracts.V1.Requests.Post;
using Bingo.Contracts.V1.Requests.User;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.Identity;
using Bingo.Contracts.V1.Responses.Post;
using BingoAPI;
using BingoAPI.Data;
using BingoAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Bingo.IntegrationTests
{
    
    public class IntegrationTest
    {
        protected readonly HttpClient TestClient;
        private readonly string _token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbmlzdHJhdGlvbkBob3BhdXQuY29tIiwianRpIjoiNmMxOWYzM2UtMzA2OC00OGIzLWJiY2EtZjAxYTViZGZmYzA4IiwiZW1haWwiOiJhZG1pbmlzdHJhdGlvbkBob3BhdXQuY29tIiwiaWQiOiJkNjFkNWJhMS00MWNhLTQ0ZjMtOTI3NC05YmUyN2JmZjE1MTIiLCJyb2xlIjpbIkFkbWluIiwiVXNlciIsIlN1cGVyQWRtaW4iXSwibmJmIjoxNTk0MjE4MDk1LCJleHAiOjE1OTQyNTQwOTUsImlhdCI6MTU5NDIxODA5NX0.vCxXn-63dZV6Jk32K_qV89fWo9EZ0RZ6ryZP-JfPYpM";
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
