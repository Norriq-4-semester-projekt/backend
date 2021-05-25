﻿using DataAccess.Entities;
using FluentValidation;

namespace DataAccess
{
    public class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {
            RuleFor(user => user.Password).MinimumLength(8).WithMessage("Must be at least 8 characters").NotEmpty().WithMessage("Must be at least 8 characters").Matches("[0-9]").WithMessage("Password must contain a number");
            RuleFor(user => user.Username).NotEmpty().WithMessage("Username can't be empty");
        }
    }
}