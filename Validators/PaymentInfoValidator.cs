using FluentValidation;
using Nop.Plugin.Payments.Stripe.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Payments.Stripe.Validators
{
    public partial class PaymentInfoValidator : BaseNopValidator<PaymentInfoModel>
    {
        public PaymentInfoValidator(ILocalizationService localizationService)
        {
            //RuleFor(x => x.AccountId).NotEmpty().WithMessage(localizationService.GetResource("Payment.CardNumber.Wrong"));
        }
    }
}