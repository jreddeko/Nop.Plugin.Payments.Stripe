using Nop.Core.Data;
using Nop.Plugin.Payments.Stripe.Domain;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.Stripe.Services
{
    public class PaymentStripeEventService : IPaymentStripeEventService
    {
        private IRepository<PaymentStripeEvent> _paymentStripeEventRepository;
        private StripeEventService _stripeEventService;

        public PaymentStripeEventService(StripeEventService stripeEventService,
            IRepository<PaymentStripeEvent> paymentStripeEventRepository)
        {
            _stripeEventService = stripeEventService;
            _paymentStripeEventRepository = paymentStripeEventRepository;
        }

        public IEnumerable<StripeEvent> GetAllEvents(string accountId)
        {
            var requestOptions = new StripeRequestOptions()
            {
                StripeConnectAccountId = accountId,
            };            
                        
            return _stripeEventService
                .List(null, requestOptions)
                .AsEnumerable();
        }

        /// <summary>
        /// Stores id of event to signify that the event has 
        /// already been processed.
        /// </summary>
        /// <param name="stripeEvent"></param>
        public void InsertProcessedEvent(StripeEvent stripeEvent)
        {
            var p = new PaymentStripeEvent
            {
                EventId = stripeEvent.Id,
            };
            _paymentStripeEventRepository.Insert(p);
        }

        /// <summary>
        /// Takes json string and returns StripeEvent
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public StripeEvent ParseEvent(string request)
        {
            var stripeEvent = StripeEventUtility.ParseEvent(request);
            return stripeEvent;
        }

        public bool IsEventNew(StripeEvent stripeEvent)
        {
            return !_paymentStripeEventRepository
                .Table
                .Any(p => p.EventId == stripeEvent.Id);
        }

        public void ProcessEvent(StripeEvent stripeEvent)
        {
            // process event

            //balance.available
            //charge.dispute.created
            //charge.dispute.closed
            //charge.dispute.updated
            // charge.refund.updated

        }
    }
}
