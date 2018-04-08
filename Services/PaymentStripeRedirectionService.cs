using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Payments;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Nop.Plugin.Payments.Stripe.Services
{
    public class PaymentStripeRedirectionService : IPaymentStripeRedirectionService
    {
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly HttpSessionStateBase _session;
        private readonly IPaymentStripeService _stripeService;
        private readonly StripePaymentSettings _stripePaymentSettings;
        private readonly IPaymentStripeCheckoutDetailsService _stripeCheckoutDetailsService;

        public PaymentStripeRedirectionService( ILogger logger,
            IWebHelper webHelper,
            IWorkContext workContext,
            ICustomerService customerService,
            HttpSessionStateBase session,
            IPaymentStripeService stripeService,
            IPaymentStripeCheckoutDetailsService stripeCheckoutDetailsService,
            StripePaymentSettings stripePaymentSettings)
        {
            _logger = logger;
            _webHelper = webHelper;
            _workContext = workContext;
            _customerService = customerService;
            _session = session;
            _stripeService = stripeService;
            _stripePaymentSettings = stripePaymentSettings;
            _stripeCheckoutDetailsService = stripeCheckoutDetailsService;
        }

        /// <summary>
        /// Webhook returned after Stripe processes 3d secure payments
        /// </summary>
        /// <param name="source"></param>
        /// <param name="accountId"></param>
        /// <param name="redirectUrl"></param>
        /// <returns></returns>
        public bool ProcessReturn(string sourceId)
        {
            if (String.IsNullOrWhiteSpace(sourceId))
                return false;

            var source = _stripeService.GetSource(sourceId, _stripePaymentSettings.ConnectAccountId, null);

            if (source == null)
                return false;
            var processPaymentRequest = _stripeCheckoutDetailsService.SetCheckoutDetails(source);
            _session["OrderPaymentInfo"] = processPaymentRequest;
            return true;
        }
    }
}
