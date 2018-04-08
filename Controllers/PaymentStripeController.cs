using Nop.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Nop.Core;
using Nop.Services.Stores;
using Nop.Services.Configuration;
using Nop.Services.Orders;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.Stripe.Validators;
using Nop.Plugin.Payments.Stripe.Models;
using Stripe;
using Nop.Plugin.Payments.Stripe.Services;
using Nop.Services.Directory;
using Nop.Services.Shipping;
using Nop.Plugin.Payments.Stripe.Factories;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Nop.Services.Payments;
using Nop.Web.Framework.Kendoui;

namespace Nop.Plugin.Payments.Stripe.Controllers
{
    public class PaymentStripeController : BasePaymentController
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;
        private readonly ISettingService _settingService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly PaymentSettings _paymentSettings;
        private readonly StripePaymentSettings _stripePaymentSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;

        private readonly IPaymentStripeCheckoutService _paymentStripeCheckoutService;
        private readonly IPaymentStripeCheckoutPlaceOrderService _paymentStripeCheckoutPlaceOrderService;
        private readonly IPaymentStripeRedirectionService _paymentStripeRedirectionService;
        private readonly IPaymentStripeService _paymentStripeService;
        private readonly IPaymentStripeFactory _paymentStripeFactory;
        private readonly IPaymentStripeEventService _paymentStripeEventService;

        #endregion

        #region Ctor

        public PaymentStripeController(IWorkContext workContext,
            Services.IPaymentStripeService stripeService,
            IShoppingCartService shoppingCartService,
            IStoreService storeService,
            ISettingService settingService,
            IOrderService orderService,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IOrderProcessingService orderProcessingService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            ILogger logger,
            IWebHelper webHelper,
            PaymentSettings paymentSettings,
            StripePaymentSettings stripePaymentSettings,
            ShoppingCartSettings shoppingCartSettings,
            IPaymentStripeFactory paymentStripeFactory,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPaymentStripeCheckoutService paymentStripeCheckoutService,
            IPaymentStripeCheckoutPlaceOrderService paymentStripeCheckoutPlaceOrderService,
            IPaymentStripeRedirectionService paymentStripeRedirectionService,
            IPaymentStripeEventService paymentStripeEventService)
        {
            this._workContext = workContext;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._paymentStripeService = stripeService;
            this._storeService = storeService;
            this._settingService = settingService;
            this._orderService = orderService;
            this._paymentStripeFactory = paymentStripeFactory;
            this._orderProcessingService = orderProcessingService;
            this._genericAttributeService = genericAttributeService;
            this._localizationService = localizationService;
            this._storeContext = storeContext;
            this._logger = logger;
            this._webHelper = webHelper;
            this._paymentSettings = paymentSettings;
            this._stripePaymentSettings = stripePaymentSettings;
            this._shoppingCartSettings = shoppingCartSettings;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._paymentStripeCheckoutService = paymentStripeCheckoutService;
            this._paymentStripeCheckoutPlaceOrderService = paymentStripeCheckoutPlaceOrderService;
            this._paymentStripeRedirectionService = paymentStripeRedirectionService;
            _paymentStripeEventService = paymentStripeEventService;
        }
        #endregion

        #region Methods

        [AdminAuthorize]
        public ActionResult Configure()
        {
            var account = _paymentStripeService.GetAccount(_stripePaymentSettings.ConnectAccountId);
            if (account == null && !String.IsNullOrEmpty(_stripePaymentSettings.ConnectAccountId))
                ErrorNotification(String.Format("Error connecting to account, please check system logs"));
            var model = _paymentStripeFactory.PrepareConfigurationModel(account);
            return View("~/Plugins/Payments.Stripe/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAuthorize]
        public ActionResult Configure(ConfigureModel model)
        {
            try
            {
                _stripePaymentSettings.ConnectAccountId = model.AccountId;

                //save settings         
                _settingService.SaveSetting(_stripePaymentSettings);

                //now clear settings cache
                _settingService.ClearCache();

                var account = _paymentStripeService.GetAccount(model.AccountId);
                if (!account.PayoutsEnabled)
                    ErrorNotification(_localizationService.GetResource("Account.Register.Unsuccessful"));
                else
                    SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
            }
            catch (Exception ex)
            {
                _logger.Error("Error posting Payments.Stripe configuration", ex);
                ErrorNotification(ex.Message, true);
            }

            return View("~/Plugins/Payments.Stripe/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public ActionResult WebhookEventHandler()
        {
            var request = new StreamReader(Request.InputStream).ReadToEnd();
            try
            {
                var stripeEvent = _paymentStripeEventService.ParseEvent(request);
                if (_paymentStripeEventService.IsEventNew(stripeEvent))
                {
                    _paymentStripeEventService.ProcessEvent(stripeEvent);
                    _paymentStripeEventService.InsertProcessedEvent(stripeEvent);
                    var message = String.Format("{0}:{1}", stripeEvent.Type, JsonConvert.SerializeObject(stripeEvent.Data));
                    _logger.Debug(message);
                }               
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                _logger.Error("Error processing Webhook Event: \"" + request + "\"", ex);
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        public JsonResult GetAllWebhooks(DataSourceRequest command)
        {
            var events = _paymentStripeEventService.GetAllEvents(_stripePaymentSettings.ConnectAccountId);
            var gridModel = new DataSourceResult
            {
                Data = events.Select(x => new {
                    Account = x.Account,
                    ApiVersion = x.ApiVersion,
                    Created = x.Created,
                    Data = JsonConvert.SerializeObject(x.Data),
                    Id = x.Id,
                    LiveMode = x.LiveMode,
                    Object = x.Object,
                    PendingWebhooks = x.PendingWebhooks,
                    Request = x.Request.IdempotencyKey,
                    RequestId = x.RequestId,
                    StripeResponse = x.StripeResponse,
                    Type = x.Type,
                }),
                Total = events.Count()
            };

            return Json(gridModel);
        }

        [HttpPost, ActionName("Confirm")]
        [ValidateInput(false)]
        public ActionResult ConfirmOrder(CheckoutConfirmModel model)
        {
            //validation
            var cart = _paymentStripeCheckoutService.GetCart();
            if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if (!_paymentStripeCheckoutService.IsAllowedToCheckout())
                return new HttpUnauthorizedResult();

            //model
            var checkoutPlaceOrderModel = _paymentStripeCheckoutPlaceOrderService.PlaceOrder();
            if (checkoutPlaceOrderModel.RedirectToCart)
                return RedirectToRoute("ShoppingCart");

            if (checkoutPlaceOrderModel.IsRedirected)
                return Content("Redirected");

            if (checkoutPlaceOrderModel.CompletedId.HasValue)
                return RedirectToRoute("CheckoutCompleted", new { orderId = checkoutPlaceOrderModel.CompletedId });

            //If we got this far, something failed, redisplay form
            return View("~/Plugins/Payments.Stripe/Views/Confirm.cshtml", checkoutPlaceOrderModel);
        }

        public ActionResult Confirm()
        {
            //validation
            var cart = _paymentStripeCheckoutService.GetCart();
            if (cart.Count == 0)
                return RedirectToRoute("ShoppingCart");

            if (!_paymentStripeCheckoutService.IsAllowedToCheckout())
                return new HttpUnauthorizedResult();

            //model
            var model = _paymentStripeFactory.PrepareConfirmOrderModel(cart);

            return View("~/Plugins/Payments.Stripe/Views/Confirm.cshtml", model);
        }

        public ActionResult Return(string client_secret, bool? livemode, string source, string accountId, string orderNumber)
        {
            var success = _paymentStripeRedirectionService.ProcessReturn(source);
            return success
                       ? RedirectToAction("Confirm")
                       : RedirectToRoute("ShoppingCart");
        }

        public ActionResult StripeOAuth(string code, string scope)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var values = new Dictionary<string, string>
                    {
                       { "client_secret", "" }, // test key 
                       { "code", code },
                       { "grant_type", "authorization_code" },
                    };
                    
                    // Delete before going live
                    System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    var content = new FormUrlEncodedContent(values);
                    var response = client.PostAsync("https://connect.stripe.com/oauth/token", content).Result;
                    var responseString = response.Content.ReadAsStringAsync().Result;
                    var jObject = JObject.Parse(responseString);
                    var jAccountIdToken = jObject.GetValue("stripe_user_id");
                    var jErrorToken = jObject.GetValue("error");
                    if (jErrorToken != null)
                    {
                        _logger.Error(String.Format("Stripe Error Found: '{0}'", jErrorToken.ToString()));
                        var jErrorMessageToken = jObject.GetValue("error_description");
                        ErrorNotification(jErrorMessageToken.ToString());
                        return RedirectToAction("ConfigureMethod", "Payment", new { area = "Admin", systemName = "Payments.Stripe" });
                    }
                    var accountId = jAccountIdToken.ToString();
                    _stripePaymentSettings.ConnectAccountId = accountId;
                    _settingService.SaveSetting(_stripePaymentSettings);

                    var account = _paymentStripeService.GetAccount(accountId);
                    if (account == null)
                    {
                        ErrorNotification("Stripe account not found");
                        return RedirectToAction("ConfigureMethod", "Payment", new { area = "Admin", systemName = "Payments.Stripe" });
                    }

                    if (account.PayoutsEnabled)
                        SuccessNotification("Stripe account successfully configured for payouts");
                    else
                        ErrorNotification("Stripe account is not configured for payouts");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(String.Format("Error completing OAuth callback from Stripe.  Code: '{0}'", code), ex);
                ErrorNotification(ex.Message, true);
            }
            return RedirectToAction("ConfigureMethod", "Payment", new { area = "Admin", systemName = "Payments.Stripe" });
        }

        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            var returnUrl = this.Url.Action("Return", "PaymentStripe", new { accountId = _stripePaymentSettings.ConnectAccountId }, this.Request.Url.Scheme);
            _stripePaymentSettings.ReturnUrl = _webHelper.GetStoreLocation(false) + "Plugins/PaymentStripe/ProcessPayment";
            _settingService.SaveSetting(_stripePaymentSettings);
            var cart = _workContext.CurrentCustomer.ShoppingCartItems.ToList();
            var model = new PaymentInfoModel()
            {
                StripePublicApiKey = _paymentStripeService.GetPublicApiKey(),
                Amount = Decimal.ToInt32(_orderTotalCalculationService.GetShoppingCartTotal(cart) * 100 ?? 0),
                CustomerEmail = _workContext.CurrentCustomer.Email,
                AccountId = _stripePaymentSettings.ConnectAccountId,
                ReturnUrl = returnUrl,
            };
            return View("~/Plugins/Payments.Stripe/Views/PaymentInfo.cshtml", model);
        }

        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            paymentInfo.CustomValues.Add("StripeSource", form["StripeSource"]);
            paymentInfo.CustomValues.Add("RedirectUri", this.Url.Action("PaymentInfo", "PaymentStripe", new { accountId = _stripePaymentSettings.ConnectAccountId }, this.Request.Url.Scheme));
            return paymentInfo;
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        #endregion
    }
}
