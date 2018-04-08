using Nop.Services.Payments;
using Stripe;

namespace Nop.Plugin.Payments.Stripe.Services
{
    public interface IPaymentStripeCheckoutDetailsService
    {
        ProcessPaymentRequest SetCheckoutDetails(StripeSource stripeSource);
    }
}