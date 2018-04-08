using Nop.Web.Framework.Mvc.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace Nop.Plugin.Payments.Stripe
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {

            //ConnectedAccount OAuth Redirect Flow
            routes.MapRoute("Plugin.Payments.Stripe.Return",
                 "Plugins/PaymentStripe/Return",
                 new { controller = "PaymentStripe", action = "Return" },
                 new[] { "Nop.Plugin.Payments.Stripe.Controllers" }
            );

            //ConnectedAccount OAuth Redirect Flow
            routes.MapRoute("Plugin.Payments.Stripe.Confirm",
                 "Plugins/PaymentStripe/Confirm",
                 new { controller = "PaymentStripe", action = "Confirm" },
                 new[] { "Nop.Plugin.Payments.Stripe.Controllers" }
            );

            //ConnectedAccount OAuth Redirect Flow
            routes.MapRoute("Plugin.Payments.Stripe.SubmitButton",
                 "Plugins/PaymentStripe/SubmitButton",
                 new { controller = "PaymentStripe", action = "SubmitButton" },
                 new[] { "Nop.Plugin.Payments.Stripe.Controllers" }
            );

            //ConnectedAccount OAuth Redirect Flow
            routes.MapRoute("Plugin.Payments.Stripe.StripeOAuth",
                 "Plugins/PaymentStripe/StripeOAuth",
                 new { controller = "PaymentStripe", action = "StripeOAuth" },
                 new[] { "Nop.Plugin.Payments.Stripe.Controllers" }
            );
            //PDT
            routes.MapRoute("Plugin.Payments.Stripe.ProcessPayment",
                 "Plugins/PaymentStripe/ProcessPayment",
                 new { controller = "PaymentStripe", action = "ProcessPayment" },
                 new[] { "Nop.Plugin.Payments.Stripe.Controllers" }
            );
            //Cancel
            routes.MapRoute("Plugin.Payments.Stripe.CancelOrder",
                 "Plugins/PaymentStripe/CancelOrder",
                 new { controller = "PaymentStripe", action = "CancelOrder" },
                 new[] { "Nop.Plugin.Payments.Stripe.Controllers" }
            );

            //WebhookEventHandler
            routes.MapRoute("Plugin.Payments.Stripe.WebhookEventHandler",
                 "Plugins/PaymentStripe/WebhookEventHandler",
                 new { controller = "PaymentStripe", action = "WebhookEventHandler" },
                 new[] { "Nop.Plugin.Payments.Stripe.Controllers" }
            );

            //GetAllWebhooks
            routes.MapRoute("Plugin.Payments.Stripe.GetAllWebhooks",
                 "Plugins/PaymentStripe/GetAllWebhooks",
                 new { controller = "PaymentStripe", action = "GetAllWebhooks" },
                 new[] { "Nop.Plugin.Payments.Stripe.Controllers" }
            );
        }

        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
