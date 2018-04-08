using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stripe;
using System.ComponentModel;
using System.Web.Mvc;
using Nop.Web.Framework;

namespace Nop.Plugin.Payments.Stripe.Models
{
    public class ConfigureModel
    {
        [NopResourceDisplayName("Plugins.Payments.Stripe.AccountId")]
        public string AccountId { get; set; }

        [NopResourceDisplayName("Plugins.Payments.Stripe.PaymentsEnabled")]
        public bool PaymentsEnabled { get; set; }
        public string RedirectUri { get; set; }
    }
}
