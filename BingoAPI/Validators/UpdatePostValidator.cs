using Bingo.Contracts.V1.Requests.Post;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.Validators
{
    public class UpdatePostValidator : AbstractValidator<UpdatePostRequest>
    {
        public UpdatePostValidator()
        {
            RuleFor(x => x.EventTime)
                 .GreaterThan(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 600)
                 .WithMessage("Event starting time can't be in the past");

            RuleFor(x => x.EndTime)
                 .GreaterThanOrEqualTo(x => x.EventTime + 900)
                 .WithMessage("Event should last at least 15 min")
                 .LessThan(x => x.EventTime + 43200)
                 .WithMessage("Event duration time limited to 12h");
        }
    }
}
