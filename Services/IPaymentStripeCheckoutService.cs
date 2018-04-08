using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Payments.Stripe.Services
{
    public interface IPaymentStripeCheckoutService
    {
        IList<ShoppingCartItem> GetCart();
        IEnumerable<SelectListItem> GetLocaleCodeOptions(string localeCode);
        bool IsAllowedToCheckout();
        bool IsMinimumOrderPlacementIntervalValid(Customer customer);
    }
}