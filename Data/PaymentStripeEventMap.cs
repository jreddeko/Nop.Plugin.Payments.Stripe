using Nop.Data.Mapping;
using Nop.Plugin.Payments.Stripe.Domain;

namespace Nop.Plugin.Payments.Stripe.Data
{
    public partial class PaymentStripeEventMap : NopEntityTypeConfiguration<PaymentStripeEvent>
    {
        public PaymentStripeEventMap()
        {
            this.ToTable("PaymentStripeEvent");
            this.HasKey(tr => tr.Id);
        }
    }
}