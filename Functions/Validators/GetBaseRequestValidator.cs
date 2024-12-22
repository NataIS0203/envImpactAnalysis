using Durable.Functions.Models;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Durable.Functions.Validators
{
    public class GetBaseRequestValidator : AbstractValidator<GetBaseRequest>
    {
        public GetBaseRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithErrorCode(StatusCodes.Status400BadRequest.ToString());
            RuleFor(x => x.ReportName)
                .NotEmpty()
                .WithErrorCode(StatusCodes.Status400BadRequest.ToString());
            RuleFor(x => x.Percentage)
                .Must(x => int.TryParse(x, out int z))
                .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
                .When(x => x.Percentage != null);
        }
    }
}
