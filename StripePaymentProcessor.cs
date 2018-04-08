using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using Nop.Plugin.Payments.Stripe.Controllers;
using Nop.Plugin.Payments.Stripe.Data;
using Nop.Plugin.Payments.Stripe.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Tax;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;

namespace Nop.Plugin.Payments.Stripe
{

    /// <summary>
    /// Stripe payment processor
    /// </summary>
    public class StripePaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly HttpContextBase _httpContext;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICurrencyService _currencyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ISettingService _settingService;
        private readonly ITaxService _taxService;
        private readonly IWebHelper _webHelper;
        private readonly StripePaymentSettings _stripePaymentSettings;
        private Services.IPaymentStripeService _stripeService;
        private IWorkContext _workContext;
        private PaymentStripeObjectContext _objectContext;

        #endregion

        #region Ctor

        public StripePaymentProcessor(CurrencySettings currencySettings,
            Services.IPaymentStripeService stripeService,
            HttpContextBase httpContext,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICurrencyService currencyService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IOrderTotalCalculationService orderTotalCalculationService,
            ISettingService settingService,
            ITaxService taxService,
            IWebHelper webHelper,
            StripePaymentSettings stripePaymentSettings,
            PaymentStripeObjectContext objectContext,
            IWorkContext workContext)
        {
            _currencySettings = currencySettings;
            _httpContext = httpContext;
            _checkoutAttributeParser = checkoutAttributeParser;
            _currencyService = currencyService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _settingService = settingService;
            _taxService = taxService;
            _webHelper = webHelper;
            _stripePaymentSettings = stripePaymentSettings;
            _stripeService = stripeService;
            _workContext = workContext;
            _objectContext = objectContext;

            // Set your secret key: remember to change this to your live secret key in production
            // See your keys here: https://dashboard.stripe.com/account/apikeys
            StripeConfiguration.SetApiKey(_stripePaymentSettings.StripeFrameworkSecretKey);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            _objectContext.Install();
            //settings
            var settings = new StripePaymentSettings
            {
                StripeFrameworkSecretKey = "", // enter secret key here
                StripeFrameworkPublicKey = "", // enter public key here
                ConnectAccountId = "",
            };

            _settingService.SaveSetting(settings);

            //locales
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Stripe.PaymentMethodDescription", "Payment Information");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Stripe.ConfigureDescription", "<p>A Standard Stripe account is a conventional Stripe account controlled directly by the account holder (i.e., you). A user with a Standard account has a relationship with Stripe, is able to log in to the Dashboard, can process charges on their own, and can disconnect their account from this platform.</p><br/><p>  <strong>Step 1:</strong> Click the link to connect with Stripe: </p><p>  Click the Connect with Stripe button below to begin the connection.</p><br/><p>  <strong>Step 2:</strong> Create or connect your Stripe account:</p><p>After you click the link, you will be taken to Stripe's website where you will be prompted to allow or deny the connection to your platform.</p><br/><p>  <strong>Step 3:</strong> Return to this configuration page:</p><p>  After you connect your existing or newly created account, you will redirected back to your site.</p><br/><p>  <strong>Step 4:</strong> We process your Stripe account:</p><p>  Assuming no errors occurred, the last step is for us to fetch your Stripe credentials and validate if payments are enabled on your account. If any errors are found review your accounts information at https://stripe.com/, if further errors persist contact  https://support.stripe.com/.</p>");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Stripe.AccountId", "Account Id");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Stripe.PaymentsEnabled", "Payments Enabled?");
            this.AddOrUpdatePluginLocaleResource("Plugins.Payments.Stripe.Fields.RedirectionTip", "Payments Enabled?");

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            _objectContext.Uninstall();
            //settings
            _settingService.DeleteSetting<StripePaymentSettings>();

            //locales
            this.DeletePluginLocaleResource("Plugins.Payments.Stripe.PaymentMethodDescription");
            this.DeletePluginLocaleResource("Plugins.Payments.Stripe.ConfigureDescription");
            this.DeletePluginLocaleResource("Plugins.Payments.Stripe.AccountId");
            this.DeletePluginLocaleResource("Plugins.Payments.Stripe.PaymentsEnabled");

            base.Uninstall();
        }


        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            return true;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }


        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var customers = new StripeCustomerService();
            var charges = new StripeChargeService();

            var stripeSourceId = processPaymentRequest.CustomValues["StripeSourceId"] as string;

            var result = new ProcessPaymentResult();
            var stripeCharge = _stripeService.ChargeAmount(stripeSourceId, 
                _stripePaymentSettings.ConnectAccountId, 
                processPaymentRequest.OrderGuid.ToString(),
                Decimal.ToInt32(processPaymentRequest.OrderTotal * 100));
            if (stripeCharge.Status == "succeeded")
            {
                result.NewPaymentStatus = PaymentStatus.Paid;
            }
            else
            {
                result.NewPaymentStatus = PaymentStatus.Voided;
                result.AddError(stripeCharge.FailureMessage);
            }
            return result;
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription
        {
            //return description of this payment method to be display on "payment method" checkout step. good practice is to make it localizable
            //for example, for a redirection payment method, description may be like this: "You will be redirected to PayPal site to complete the payment"
            get { return _localizationService.GetResource("Plugins.Payments.Stripe.PaymentMethodDescription"); }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get { return PaymentMethodType.Button; }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get { return RecurringPaymentType.NotSupported; }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get { return false; }
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var url = String.Format("", // local url
                Decimal.ToInt32(postProcessPaymentRequest.Order.OrderTotal * 100),
                _stripePaymentSettings.ConnectAccountId,
                _stripePaymentSettings.ReturnUrl,
                postProcessPaymentRequest.Order.OrderGuid);
            _httpContext.Response.Redirect(url);
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            var result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart,
                0, false);
            return result;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "PaymentStripe";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.Stripe.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "PaymentStripe";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.Stripe.Controllers" }, { "area", null } };
        }


        /// <summary>
        /// Get type of controller
        /// </summary>
        /// <returns>Type</returns>
        public Type GetControllerType()
        {
            return typeof(PaymentStripeController);
        }

        /// <summary>
        /// Gets a payment status
        /// </summary>
        /// <param name="paymentStatus">PayPal payment status</param>
        /// <param name="pendingReason">PayPal pending reason</param>
        /// <returns>Payment status</returns>
        public PaymentStatus GetPaymentStatus(string paymentStatus)
        {
            var result = PaymentStatus.Pending;

            if (paymentStatus == null)
                paymentStatus = string.Empty;

            switch (paymentStatus.ToLowerInvariant())
            {
                case "pending":
                    result = PaymentStatus.Pending;
                    break;
                case "succeeded":
                    result = PaymentStatus.Paid;
                    break;
                case "failed":
                    result = PaymentStatus.Voided;
                    break;
                default:
                    break;
            }
            return result;
        }
        #endregion
    }
}
