using Bingo.Contracts.V1.Requests.Post;
using FluentValidation;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Validators
{
    public class CreatePostValidator : AbstractValidator<CreatePostRequest>
    {
        public CreatePostValidator()
        {
            this.CascadeMode = CascadeMode.StopOnFirstFailure;
                      
            RuleFor(x => x.EventTime)
                 .GreaterThan(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 1800)
                 .WithMessage("Event starting time can't be in the past");

            RuleFor(x => x.EndTime)
                 .GreaterThanOrEqualTo(x => x.EventTime + 900)
                 .WithMessage("Event should last at least 15 min")
                 .LessThan(x => x.EventTime + 43200)
                 .WithMessage("Event duration time limited to 12h");

            RuleFor(x => x.UserLocation.Longitude)
                .GreaterThanOrEqualTo(-180)
                .LessThanOrEqualTo(180);

            RuleFor(x => x.UserLocation.Latitude)
                .GreaterThanOrEqualTo(-90)
                .LessThanOrEqualTo(90);
                        

            RuleFor(x => x.Event.Description)
                .MinimumLength(10)
                .WithMessage("Description length at least 10 characters")
                .MaximumLength(5000)
                .WithMessage("Description length at most 5000 characters");

            RuleFor(x => x.Event.EventType)
                 .LessThanOrEqualTo(9)
                 .GreaterThanOrEqualTo(1);

            RuleFor(x => x.Tags.All(x => x.Length < 20));                                    
        }
                
    }
}
