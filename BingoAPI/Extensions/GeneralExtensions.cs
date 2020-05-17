using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
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
            if (httpContext.User == null)
            {
                return string.Empty;
            }
            return httpContext.User.Claims.Single(x => x.Type == "id").Value;
        }

        public static void AddAllIfNotNull<T>(this List<T> list, IEnumerable<T> values)
        {
            foreach(var value in values)
            {
                if (value != null)
                {
                    list.Add(value);
                }
            }
        }
               
    }
}
