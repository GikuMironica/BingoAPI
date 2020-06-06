using Bingo.Contracts.V1.Requests.Post;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.CustomValidation
{
    public class UpdatedPostDetailsWatcher : IUpdatedPostDetailsWatcher
    {
        public bool GetValidatedFields(UpdatePostRequest updatePostRequest)
        {
            Dictionary<string, bool> updatedFields = new Dictionary<string, bool>();

            updatedFields.Add("location", updatePostRequest.UserLocation != null);
            updatedFields.Add("event time", updatePostRequest.EventTime != null);
            if(updatePostRequest.Event != null)
            {
                updatedFields.Add("requirements", updatePostRequest.Event.Requirements != null);
                updatedFields.Add("entrance price", updatePostRequest.Event.EntrancePrice != null);
            }
            

            foreach(var field in updatedFields)
            {
                if (field.Value)
                {
                    return true;
                }
            }
            return false;            
        }
    }
}
