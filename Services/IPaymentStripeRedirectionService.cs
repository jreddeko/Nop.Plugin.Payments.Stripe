using Stripe;

namespace Nop.Plugin.Payments.Stripe.Services
{
    public interface IPaymentStripeRedirectionService
    {
        bool ProcessReturn(string sourceId);
    }
}