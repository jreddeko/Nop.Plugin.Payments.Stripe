using System.Collections.Generic;
using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.Stripe
{
    public class StripePaymentSettings : ISettings
    {
        public string ConnectAccountId { get; set; }
        public IList<string> Issues { get; internal set; }
        public string StripeFrameworkPublicKey { get; set; }
        public string StripeFrameworkSecretKey { get; set; }
        public string ReturnUrl { get; set; }
    }
}