using Api.Models.v1_0;
using FluentValidation;
using DataAccess.Entities;

namespace Api.Helpers
{
    public class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {
            RuleFor(user => user.Password).MinimumLength(8).WithMessage("Must be at least 8 charecters").NotEmpty().WithMessage("Must be at least 8 charecters").Matches("[0-9]").WithMessage("Password must contain a number");
            RuleFor(user => user.Username).NotEmpty().WithMessage("Username can't be empty");
        }
    }
}
