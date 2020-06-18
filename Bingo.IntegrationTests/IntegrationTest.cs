using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.Identity;
using Bingo.Contracts.V1.Requests.Post;
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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Bingo.IntegrationTests
{
    
    public class IntegrationTest
    {
        protected readonly HttpClient TestClient;
        private readonly string _token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbmlzdHJhdGlvbkBob3BhdXQuY29tIiwianRpIjoiZjE1MDJmMDAtYmUxOC00MTc4LTk4YWUtZDRhZDkwOWQ2OTQwIiwiZW1haWwiOiJhZG1pbmlzdHJhdGlvbkBob3BhdXQuY29tIiwiaWQiOiJkNjFkNWJhMS00MWNhLTQ0ZjMtOTI3NC05YmUyN2JmZjE1MTIiLCJyb2xlIjpbIkFkbWluIiwiVXNlciIsIlN1cGVyQWRtaW4iXSwibmJmIjoxNTkyNTA3NzcwLCJleHAiOjE1OTI1MjU3NzAsImlhdCI6MTU5MjUwNzc3MH0.dsc6t5bI-5pujYTAP4IW2TV5agSfm-OO33dt5ILLoI0";

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

        protected async Task AuthenticateAsync()
        {
            TestClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", await GetJwtAsync());
        }

        protected async Task AuthenticateAdminAsync()
        {
            TestClient.DefaultRequestHeaders.Authorization =
               new AuthenticationHeaderValue("bearer", _token);
        }       

        private async Task<string> GetJwtAsync()
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

            // set admin jwt token in order to confirm the email
            TestClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("bearer", _token);
            var result = await TestClient.GetAsync(ApiRoutes.Identity.AdminConfirmEmail.Replace("{email}", loginRequest.Email));

            var loginResponse = await TestClient.PostAsJsonAsync(ApiRoutes.Identity.Login, loginRequest);

            var registrationResponse = await loginResponse.Content.ReadFromJsonAsync<AuthSuccessResponse>();
            return registrationResponse.Token;
        }
    }
}
