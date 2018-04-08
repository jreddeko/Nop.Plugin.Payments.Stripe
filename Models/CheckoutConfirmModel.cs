using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.Stripe.Models
{
    public partial class CheckoutConfirmModel : BaseNopModel
    {
        public CheckoutConfirmModel()
        {
            Warnings = new List<string>();
        }

        public string MinOrderTotalWarning { get; set; }
        public IList<string> Warnings { get; set; }
    }

    public class CheckoutPlaceOrderModel : CheckoutConfirmModel
    {
        public bool RedirectToCart { get; set; }

        public bool IsRedirected { get; set; }

        public int? CompletedId { get; set; }
    }
}
