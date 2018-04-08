using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.Stripe.Models;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Nop.Plugin.Payments.Stripe.Factories
{
    public class PaymentStripeFactory : IPaymentStripeFactory
    {
        private readonly ICountryService _countryService;
        private readonly ILocalizationService _localizationService;
        private readonly ICurrencyService _currencyService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IWorkContext _workContext;
        private readonly IWebHelper _webHelper;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly OrderSettings _orderSettings;
        private readonly IPriceFormatter _priceFormatter;

        public PaymentStripeFactory(ICountryService countryService, IStateProvinceService stateProvinceService,
            ILocalizationService localizationService, IWorkContext workContext, IWebHelper webHelper,
            ICurrencyService currencyService,
            OrderSettings orderSettings,
            IPriceFormatter priceFormatter,
            IOrderProcessingService orderProcessingService)
        {
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _localizationService = localizationService;
            _workContext = workContext;
            _webHelper = webHelper;
            _orderProcessingService = orderProcessingService;
            _currencyService = currencyService;
            _orderSettings = orderSettings;
            _priceFormatter = priceFormatter;
        }

        public ConfigureModel PrepareConfigurationModel(StripeAccount account)
        {

            var model = new ConfigureModel();
            //https://connect.stripe.com/oauth/authorize?response_type=code&amp;client_id=ca_BlJVOMfIvvIZirpsUMpS1IzZB3z4QuiC&amp;scope=read_write&redirect_uri=@Url.Action("Test", "ConnectedAccount", null, this.Request.Url.Scheme)
            model.RedirectUri = "https://connect.stripe.com/oauth/authorize?response_type=code&amp;client_id=ca_BlJVOMfIvvIZirpsUMpS1IzZB3z4QuiC&amp;scope=read_write&redirect_uri=" + _webHelper.GetStoreLocation(false) + "Plugins/PaymentStripe/StripeOAuth";

            if (account != null)
            {
                model.AccountId = account.Id;
                model.PaymentsEnabled = account.PayoutsEnabled;
            }

            return model;
        }

        public CheckoutConfirmModel PrepareConfirmOrderModel(IList<ShoppingCartItem> cart)
        {
            var model = new CheckoutConfirmModel();
            //min order amount validation
            bool minOrderTotalAmountOk = _orderProcessingService.ValidateMinOrderTotalAmount(cart);
            if (!minOrderTotalAmountOk)
            {
                decimal minOrderTotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderTotalAmount, _workContext.WorkingCurrency);
                model.MinOrderTotalWarning = string.Format(_localizationService.GetResource("Checkout.MinOrderTotalAmount"), _priceFormatter.FormatPrice(minOrderTotalAmount, true, false));
            }
            return model;
        }
    }
}
