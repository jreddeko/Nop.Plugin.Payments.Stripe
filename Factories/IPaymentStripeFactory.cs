using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.Stripe.Models;
using Stripe;
using System.Collections.Generic;

namespace Nop.Plugin.Payments.Stripe.Factories
{
    public interface IPaymentStripeFactory
    {
        ConfigureModel PrepareConfigurationModel(StripeAccount account);
        CheckoutConfirmModel PrepareConfirmOrderModel(IList<ShoppingCartItem> cart);
    }
}