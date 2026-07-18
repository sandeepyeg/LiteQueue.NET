using FluentValidation;
using LiteQueue.Contracts.Messages;

namespace LiteQueue.API.Validators;

public class AcknowledgeRequestValidator : AbstractValidator<AcknowledgeRequest>
{
    public AcknowledgeRequestValidator()
    {
        RuleFor(x => x.ReceiptHandle)
            .NotEmpty().WithMessage("Receipt handle is required.");
    }
}

public class RejectRequestValidator : AbstractValidator<RejectRequest>
{
    public RejectRequestValidator()
    {
        RuleFor(x => x.ReceiptHandle)
            .NotEmpty().WithMessage("Receipt handle is required.");
    }
}
