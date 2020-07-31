using FluentValidation;

namespace Ranger.ApiGateway.Tests.Validation.Users
{
    public class DeleteUserModelValidator : AbstractValidator<DeleteUserModel>
    {
        public DeleteUserModelValidator()
        {
            RuleFor(x => x.CommandingUserEmail).NotEmpty().EmailAddress();
        }
    }
}