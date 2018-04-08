using System.Collections.Generic;
using Stripe;

namespace Nop.Plugin.Payments.Stripe.Services
{
    public interface IPaymentStripeEventService
    {
        IEnumerable<StripeEvent> GetAllEvents(string accountId);
        StripeEvent ParseEvent(string request);
        void InsertProcessedEvent(StripeEvent stripeEvent);
        bool IsEventNew(StripeEvent stripeEvent);
        void ProcessEvent(StripeEvent stripeEvent);
    }
}