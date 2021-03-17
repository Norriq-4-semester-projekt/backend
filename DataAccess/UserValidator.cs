using DataAccess.Entities;
using FluentValidation;

namespace DataAccess
{
    public class UserValidator : AbstractValidator<User>
    {   
        //Sørger for at password er mindst 8 bogstaver, og at der er et username
        public UserValidator()
        {
            RuleFor(user => user.Password).MinimumLength(8).WithMessage("Must be at least 8 charecters").NotEmpty().WithMessage("Must be at least 8 charecters").Matches("[0-9]").WithMessage("Password must contain a number");
            RuleFor(user => user.Username).NotEmpty().WithMessage("Username can't be empty");
        }
    }
}