using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Bingo.Contracts.V1;
using Bingo.Contracts.V1.Requests.Payments;
using Bingo.Contracts.V1.Responses;
using Bingo.Contracts.V1.Responses.Payments;
using BingoAPI.Domain;
using BingoAPI.Extensions;
using BingoAPI.Options;
using BingoAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace BingoAPI.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Admin,User")]
    [Produces("application/json")]
    public class PaymentsController : Controller
    {
        private readonly MyServicesHttpClient _myServicesHttpClient;
        private readonly IMapper _mapper;
        private readonly MyServicesSettings _myServicesOptions;

        public PaymentsController(MyServicesHttpClient myServicesHttpClient, IMapper mapper, IOptions<MyServicesSettings> myServicesOptions)
        {
            _myServicesHttpClient = myServicesHttpClient;
            _mapper = mapper;
            _myServicesOptions = myServicesOptions.Value;
        }


        [HttpGet(ApiRoutes.Payments.GetPaymentToken)]
        public async Task<IActionResult> GetToken(TokenRequest tokenRequest)
        {
            var httpClient = _myServicesHttpClient.HttpClient;
            
            // Shall be added only once
            if (!httpClient.IsHeaderAdded())
            {
                httpClient.DefaultRequestHeaders.Add("ApiKey", _myServicesOptions.PaymentGatewayKey);
            }

            // Set JWT
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, MyServicesRoutes.Payment.GetPaymentToken))
            {

                // Get JWT from request
                var bearerToken = Request.Headers[HeaderNames.Authorization].ToString().Replace("bearer ", "");
                httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

                using (var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage))
                {
                    if (!httpResponseMessage.IsSuccessStatusCode)
                    {
                        return BadRequest(/*Error Message has to be tunneled from Gateway*/);
                    }

                    // Deserialize
                    var gatewayResponse = await httpResponseMessage.Content!.ReadFromJsonAsync<Response<string>>();
                    return gatewayResponse != null 
                        ? Ok(new Response<GetTokenResponse> {Data = new GetTokenResponse{ PaymentToken = gatewayResponse.Data }})
                        : BadRequest(/*Error Message has to be tunneled from Gateway*/);
                }
            }
        }



        /*[HttpGet(MyServicesRoutes.Payment.Checkout)]
        public async Task<IActionResult> Checkout([FromBody] PaymentTokenRequest paymentOrder)
        {
            // Map request to domain object, get user Id & Email from JWT claims
            var requestObject = _mapper.Map<PaymentGatewayTokenRequest>(paymentOrder);
            requestObject.UserEmail = HttpContext.GetUserEmail();
            requestObject.UserId = HttpContext.GetUserId();

            var httpClient = _myServicesHttpClient.HttpClient;

            // Shall be added only once
            if (!httpClient.IsHeaderAdded())
            {
                httpClient.DefaultRequestHeaders.Add("ApiKey", _myServicesOptions.PaymentGatewayKey);
            }

            // Serialize
            var reqObj = JsonConvert.SerializeObject(requestObject);
            var data = new StringContent(reqObj, Encoding.UTF8, "application/json");

            // Send
            var response = await httpClient.PostAsync(MyServicesRoutes.Payment.GetPaymentToken, data);
            if (!response.IsSuccessStatusCode)
            {

            }
        }*/
    }
}
