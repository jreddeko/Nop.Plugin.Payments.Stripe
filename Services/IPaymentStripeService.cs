
using Stripe;

namespace Nop.Plugin.Payments.Stripe.Services
{
    public interface IPaymentStripeService
    {
        StripeCharge ChargeAmount(string sourceId, string connectAccountId, string orderNumber, int amount);
        StripeAccount GetAccount(string connectAccountId);
        string GetPublicApiKey();
        StripeSource GetSource(string sourceId, string accountId, string redirectUrl);
        StripeSource GetStripeSource3d(StripeSource source, string redirectUrl);
    }
}