using Newtonsoft.Json;
using Nop.Core.Data;
using Stripe;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Payments.Stripe.Services
{
    public class PaymentStripeService : IPaymentStripeService
    {
        private const string _publicKey = "pk_test_r3NZZSfOX9nXJjfOEXl59if7";
        private const string _privateKey = "sk_test_NaqjIjbe6gdQgDNstdNwolZl";
        private readonly StripeSourceService _stripeSourceService;
        private readonly StripeChargeService _stripeChargeService;
        private readonly StripeAccountService _stripeAccountService;
        private readonly StripeFileUploadService _stripeFileUploadService;
        private readonly StripeExternalAccountService _externalAccountService;

        public PaymentStripeService(
            StripeSourceService stripeSourceService,
            StripeChargeService stripeChargeService,
            StripeAccountService stripeAccountService,
            StripeFileUploadService stripeFileUploadService,
            StripeExternalAccountService externalAccountService)
        {
            // Set your secret key: remember to change this to your live secret key in production
            // See your keys here: https://dashboard.stripe.com/account/apikeys
            StripeConfiguration.SetApiKey(_privateKey);

            _stripeSourceService = stripeSourceService;
            _stripeChargeService = stripeChargeService;
            _stripeAccountService = stripeAccountService;
            _stripeFileUploadService = stripeFileUploadService;
            _externalAccountService = externalAccountService;
        }

        public StripeSource GetSource(string sourceId, string accountId, string redirectUrl)
        {
            var requestOptions = new StripeRequestOptions()
            {
                StripeConnectAccountId = accountId,
            };

            var source = _stripeSourceService.Get(sourceId, requestOptions);

            if (source.Flow == "redirect")
                return source;

            if (source.Card.ThreeDSecure == "required")
            {
                return this.GetStripeSource3d(source, redirectUrl);
            }
            return source;
        }

        public StripeCharge ChargeAmount(string sourceId, string connectAccountId, string orderNumber, int amount)
        {
            var requestOptions = new StripeRequestOptions()
            {
                StripeConnectAccountId = connectAccountId,
            };
            var source = _stripeSourceService.Get(sourceId, requestOptions);
            var chargeOptions = new StripeChargeCreateOptions()
            {
                Currency = "cad",
                Amount = amount,
                SourceTokenOrExistingSourceId = source.Id,
                Metadata = new Dictionary<String, String>()
                {
                    { "OrderId", orderNumber}
                }
            };
            return _stripeChargeService.Create(chargeOptions, requestOptions);
        }

        public string GetPublicApiKey()
        {
            return _publicKey;
        }

        public StripeSource GetStripeSource3d(StripeSource source, string redirectUrl)
        {
            return _stripeSourceService.Create(new StripeSourceCreateOptions()
            {
                Amount = source.Amount,
                Type = "three_d_secure",
                Currency = source.Currency,
                ThreeDSecureCardOrSourceId = source.Id,
                RedirectReturnUrl = redirectUrl, //,
            });
        }

        public StripeAccount GetAccount(string connectAccountId)
        {
            if (String.IsNullOrEmpty(connectAccountId))
                return null;
            try
            {
                return _stripeAccountService.Get(connectAccountId);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
