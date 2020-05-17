using Bingo.Contracts.V1.Requests.Post;
using FluentValidation;
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
            RuleFor(x => x.EventTime)
                .NotNull();

            RuleFor(x => x.UserLocation.Latitude)
                .NotNull();

            RuleFor(x => x.UserLocation.Longitude)
                .NotNull();

            RuleFor(x => x.Event.Description)
                .NotEmpty()
                .MinimumLength(10)
                .MaximumLength(5000);

            RuleFor(x => x.Event.Requirements)
                .MaximumLength(500);

           RuleFor(x => x.Event.EventType)
                .NotNull()
                .LessThanOrEqualTo(10)
                .GreaterThanOrEqualTo(1);

            RuleFor(x => x.Tags.All(x => x.Length < 20));

            
        }
    }
}
