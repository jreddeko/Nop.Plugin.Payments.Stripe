using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nop.Plugin.Payments.Stripe.Models
{
    public class PaymentInfoModel : BaseNopModel
    {
        public int Amount { get; set; }
        public string ErrorMessage { get; set; }
        public StripeCharge StripeCharge { get; set; }
        public string StripePublicApiKey { get; set; }
        public StripeSource StripeSource { get; internal set; }
        public string AccountId { get; set; }
        public string SourceId { get; set; }       
        public string CustomerEmail { get; set; } 
        public string ReturnUrl { get; set; }
    }
}