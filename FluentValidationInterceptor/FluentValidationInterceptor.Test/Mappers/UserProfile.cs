using FluentValidation;
using FluentValidationInterceptor.Test.Dtos;

namespace FluentValidationInterceptor.Test.Mappers
{
    public class UserProfile : AbstractValidator<CreateUserDto>
    {
        public UserProfile()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required.");
            RuleFor(x => x.Age)
                .NotNull()
                .WithMessage("Age is required.")
                .GreaterThan(0)
                .WithMessage("Age must be greater than 0.")
                .InclusiveBetween(18, 99)
                .WithMessage("Age must be between 18 and 99.");
            RuleFor(x => x.Email).EmailAddress().WithMessage("Email is not valid.");
        }
    }
}
