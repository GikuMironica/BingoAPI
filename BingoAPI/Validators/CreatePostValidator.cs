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
/*            this.CascadeMode = CascadeMode.StopOnFirstFailure;

                      
            RuleFor(x => x.EventTime)
                 .NotEqual(0);

            RuleFor(x => x.PostTime)
                 .NotEqual(0)
                 .WithMessage("Event should have a post time");

            RuleFor(x => x.UserLocation)
                 .NotNull();

            RuleFor(x => x.UserLocation.Longitude)
                .NotNull()
                .NotEqual(0);

            RuleFor(x => x.UserLocation.Latitude)
                .NotNull()
                .NotEqual(0);

            RuleFor(x => x.Event)
                 .NotNull();

            RuleFor(x => x.Event.Description)
                .NotNull()
                .MinimumLength(10)
                .MinimumLength(3000);

            RuleFor(x => x.Event.Requirements)
                .NotNull()
                .MinimumLength(500);

            RuleFor(x => x.Event.EventType)
                 .NotNull()
                 .LessThanOrEqualTo(10)
                 .GreaterThanOrEqualTo(1);

            RuleFor(x => x.Tags.All(x => x.Length < 20));*/
                                    
        }
                
    }
}
