using FluentValidation;
using LiteQueue.Contracts.Queues;

namespace LiteQueue.API.Validators;

public class CreateQueueRequestValidator : AbstractValidator<CreateQueueRequest>
{
    public CreateQueueRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Queue name is required.")
            .MaximumLength(200).WithMessage("Queue name must be 200 characters or fewer.")
            .Matches(@"^[a-zA-Z0-9\-_]+$").WithMessage("Queue name can only contain letters, numbers, hyphens, and underscores.");

        RuleFor(x => x.MaxMessageSizeBytes)
            .InclusiveBetween(1024, 10485760).When(x => x.MaxMessageSizeBytes.HasValue);
    }
}
