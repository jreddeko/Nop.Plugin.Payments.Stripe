using Nop.Plugin.Payments.Stripe.Models;

namespace Nop.Plugin.Payments.Stripe.Services
{
    public interface IPaymentStripeCheckoutPlaceOrderService
    {
        CheckoutPlaceOrderModel PlaceOrder();
    }
}