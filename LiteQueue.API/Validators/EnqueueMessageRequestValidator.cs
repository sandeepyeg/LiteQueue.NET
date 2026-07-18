using FluentValidation;
using LiteQueue.Contracts.Messages;

namespace LiteQueue.API.Validators;

public class EnqueueMessageRequestValidator : AbstractValidator<EnqueueMessageRequest>
{
    public EnqueueMessageRequestValidator()
    {
        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("Message body is required.")
            .MaximumLength(262144).WithMessage("Message body exceeds maximum size of 256 KB.");
    }
}
