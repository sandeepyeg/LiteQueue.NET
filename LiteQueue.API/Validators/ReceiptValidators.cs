using FluentValidation;

namespace LiteQueue.API.Validators;

public class AcknowledgeRequestValidator : AbstractValidator<Controllers.AcknowledgeRequest>
{
    public AcknowledgeRequestValidator()
    {
        RuleFor(x => x.ReceiptHandle)
            .NotEmpty().WithMessage("Receipt handle is required.");
    }
}

public class RejectRequestValidator : AbstractValidator<Controllers.RejectRequest>
{
    public RejectRequestValidator()
    {
        RuleFor(x => x.ReceiptHandle)
            .NotEmpty().WithMessage("Receipt handle is required.");
    }
}
