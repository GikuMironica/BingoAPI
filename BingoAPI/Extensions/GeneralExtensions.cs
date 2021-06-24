using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BingoAPI.Extensions
{
    public static class GeneralExtensions
    {
        /**
        * Method which reads the value from "id" claim passed in the JWT
        */
        public static string GetUserId(this HttpContext httpContext)
        {
            if (httpContext.User == null || !httpContext.User.Claims.Any())
            {
                return string.Empty;
            }
            return httpContext.User.Claims.Single(x => x.Type == "id").Value;
        }

        
        /// <summary>
        /// This method retrieves the user's email from the JWT
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public static string GetUserEmail(this HttpContext httpContext)
        {
            return !httpContext.User.Claims.Any() ? null : httpContext.User.Claims.Single(x => x.Type == "email").Value;
        }


        /// <summary>
        /// This method checks whether a specific header is already added to the httpClient for the next request
        /// </summary>
        /// <param name="httpClient"></param>
        public static bool IsHeaderAdded(this HttpClient httpClient)
        {
            var match = httpClient.DefaultRequestHeaders.FirstOrDefault(h => h.Key.Equals("ApiKey"));
            return match.Value != null;
        }

        public static void AddAllIfNotNull<T>(this List<T> list, IEnumerable<T> values)
        {
            foreach (var value in values)
            {
                if (value != null)
                {
                    list.Add(value);
                }
            }
        }

        public static void AddIfNotNull<T>(this List<T> list, T value)
        {
           if (value != null)
           {
              list.Add(value);
           }           
        }

    }
}
