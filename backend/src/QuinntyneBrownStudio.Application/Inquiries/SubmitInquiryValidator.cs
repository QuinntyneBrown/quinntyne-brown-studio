using System.Net.Mail;
using FluentValidation;
using QuinntyneBrownStudio.Domain.Enums;

namespace QuinntyneBrownStudio.Application.Inquiries;

public sealed class SubmitInquiryValidator : AbstractValidator<SubmitInquiry>
{
    public SubmitInquiryValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Enter your name.")
            .MaximumLength(200)
            .WithMessage("Use at most 200 characters.");
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Enter your email address.")
            .MaximumLength(254)
            .WithMessage("Use at most 254 characters.")
            .Must(email => MailAddress.TryCreate(email.Trim(), out var address) && address.Address == email.Trim())
            .WithMessage("Enter a valid email address.");
        RuleFor(x => x.Phone).MaximumLength(50).WithMessage("Use at most 50 characters.");
        RuleFor(x => x.Interest)
            .Must(interest => Enum.TryParse<ServiceKind>(interest, ignoreCase: false, out _))
            .WithMessage("Choose one of the photography services.");
        RuleFor(x => x.Message)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Tell us a little about your plans.")
            .MaximumLength(4000)
            .WithMessage("Use at most 4,000 characters.");
        RuleFor(x => x.Consent).Equal(true).WithMessage("Confirm that the studio may contact you about this request.");
    }
}
