using Bingo.Contracts.V1.Requests.Post;
using BingoAPI.Domain;
using BingoAPI.Models;
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
            if (updatePostRequest.UpdatedEvent == null) return updatedFields.Any(field => field.Value);
            updatedFields.Add("requirements", updatePostRequest.UpdatedEvent.Requirements != null);
            updatedFields.Add("entrance price", updatePostRequest.UpdatedEvent.EntrancePrice != null);


            return updatedFields.Any(field => field.Value);
        }

        public UpdatedTimeValidationResult ValidateUpdatedTime(UpdatePostRequest updatePostRequest, Post post)
        {
            // check first if end/start time were changed, if not, aprove.
            if (updatePostRequest.EventTime == null && updatePostRequest.EndTime == null)

                return new UpdatedTimeValidationResult { Result = true };

            long currentTime = DateTimeOffset.UtcNow.ToLocalTime().ToUnixTimeSeconds();

            // first case, if event didn't start yet, allow to change both end-start time
            if (post.EventTime > currentTime)
            {
                // both updated
                if(updatePostRequest.EventTime != null && updatePostRequest.EndTime != null)
                {
                    if (updatePostRequest.EventTime < DateTimeOffset.UtcNow.ToLocalTime().ToUnixTimeSeconds() - 300)
                    {
                        return new UpdatedTimeValidationResult { Result = false, ErrorMessage = "Can't postpone event to the past" };
                    }
                    if (updatePostRequest.EndTime < updatePostRequest.EventTime + 900)
                    {
                        return new UpdatedTimeValidationResult { Result = false, ErrorMessage = "Event should last at least 15 min"};
                    }
                    if (updatePostRequest.EndTime > updatePostRequest.EventTime + 43200)
                    {
                        return new UpdatedTimeValidationResult { Result = false, ErrorMessage = "Event can last at most 12h"};
                    }
                }

                // start time only provided
                if(updatePostRequest.EventTime != null && updatePostRequest.EndTime == null)
                {
                    if (updatePostRequest.EventTime < DateTimeOffset.UtcNow.ToLocalTime().ToUnixTimeSeconds() - 300)
                    {
                        return new UpdatedTimeValidationResult { Result = false, ErrorMessage = "Can't postpone event to the past" };
                    }
                    if (post.EndTime-updatePostRequest.EventTime > 43200)
                    {
                        return new UpdatedTimeValidationResult { Result = false, ErrorMessage = "Event can last at most 12h" };
                    }
                    if (post.EndTime < updatePostRequest.EventTime + 900)
                    {
                        return new UpdatedTimeValidationResult { Result = false, ErrorMessage = "Event should last at least 15 min" };
                    }
                }

                // end time only provided
                if (updatePostRequest.EventTime == null && updatePostRequest.EndTime != null)
                {
                   if(updatePostRequest.EndTime < post.EventTime + 900)
                   {
                       return new UpdatedTimeValidationResult { Result = false, ErrorMessage = "Event should last at least 15 min" };
                   }
                   if(updatePostRequest.EndTime > post.EventTime + 43200)
                   {
                       return new UpdatedTimeValidationResult { Result = false, ErrorMessage = "Event can last at most 12h" };
                   }
                }
            }

            // second case, if event started already, allow to change end time only
            if (post.EventTime > currentTime) return new UpdatedTimeValidationResult {Result = true};
            if(updatePostRequest.EventTime != null)
            {
                return new UpdatedTimeValidationResult { Result = false, ErrorMessage = "Can't change start time if event already started" };
            }
              
            if (updatePostRequest.EndTime < post.EventTime + 900)
            {
                return new UpdatedTimeValidationResult { Result = false, ErrorMessage = "Event should last at least 15 min" };
            }
            if (updatePostRequest.EndTime > post.EventTime + 43200)
            {
                return new UpdatedTimeValidationResult { Result = false, ErrorMessage = "Event can last at most 12h" };
            }
            return updatePostRequest.EndTime < DateTimeOffset.UtcNow.ToLocalTime().ToUnixTimeSeconds() + 1700 
                ? new UpdatedTimeValidationResult { Result = false, ErrorMessage = "Event can be extended by at least 30 min relative to current time" } 
                : new UpdatedTimeValidationResult { Result = true };
        }
    }
}
