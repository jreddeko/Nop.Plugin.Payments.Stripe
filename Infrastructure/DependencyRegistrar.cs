using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Payments.Stripe.Data;
using Nop.Plugin.Payments.Stripe.Domain;
using Nop.Plugin.Payments.Stripe.Factories;
using Nop.Plugin.Payments.Stripe.Services;
using Nop.Web.Framework.Mvc;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.Stripe.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        /// <summary>
        /// Register services and interfaces
        /// </summary>
        /// <param name="builder">Container builder</param>
        /// <param name="typeFinder">Type finder</param>
        /// <param name="config">Config</param>
        public virtual void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<Services.PaymentStripeService>().As<IPaymentStripeService>().InstancePerLifetimeScope();
            builder.RegisterType<PaymentStripeCheckoutPlaceOrderService>().As<IPaymentStripeCheckoutPlaceOrderService>().InstancePerLifetimeScope();
            builder.RegisterType<PaymentStripeRedirectionService>().As<IPaymentStripeRedirectionService>().InstancePerLifetimeScope();
            builder.RegisterType<PaymentStripeCheckoutService>().As<IPaymentStripeCheckoutService>().InstancePerLifetimeScope();
            builder.RegisterType<PaymentStripeFactory>().As<IPaymentStripeFactory>().InstancePerLifetimeScope();
            builder.RegisterType<PaymentStripeCheckoutDetailsService>().As<IPaymentStripeCheckoutDetailsService>().InstancePerLifetimeScope();
            builder.RegisterType<PaymentStripeEventService>().As<IPaymentStripeEventService>().InstancePerLifetimeScope();

            builder.RegisterType<StripeSourceService>();
            builder.RegisterType<StripeChargeService>();
            builder.RegisterType<StripeAccountService>();
            builder.RegisterType<StripeFileUploadService>();
            builder.RegisterType<StripeExternalAccountService>();
            builder.RegisterType<StripeEventService>();

            //data context
            this.RegisterPluginDataContext<PaymentStripeObjectContext>(builder, "nop_object_context_payment_stripe");

            //override required repository with our custom context
            builder.RegisterType<EfRepository<PaymentStripeEvent>>()
                .As<IRepository<PaymentStripeEvent>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_payment_stripe"))
                .InstancePerLifetimeScope();
        }

        /// <summary>
        /// Order of this dependency registrar implementation
        /// </summary>
        public int Order
        {
            get { return 1; }
        }
    }
}
