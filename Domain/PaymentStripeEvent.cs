using Nop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.Stripe.Domain
{
    public class PaymentStripeEvent : BaseEntity
    {
        public string EventId { get; set; }
    }
}
