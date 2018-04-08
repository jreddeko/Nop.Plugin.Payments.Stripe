using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.Stripe.Services
{
    public class PaymentStripeCheckoutDetailsService : IPaymentStripeCheckoutDetailsService
    {
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;
        private readonly IGenericAttributeService _genericAttributeService;

        public PaymentStripeCheckoutDetailsService(IWorkContext workContext, ICustomerService customerService, IStateProvinceService stateProvinceService, ICountryService countryService, IGenericAttributeService genericAttributeService)
        {
            _workContext = workContext;
            _customerService = customerService;
            _stateProvinceService = stateProvinceService;
            _countryService = countryService;
            _genericAttributeService = genericAttributeService;
        }

        public ProcessPaymentRequest SetCheckoutDetails(StripeSource stripeSource)
        {
            // get customer & cart
            //var customer = customer;
            int customerId = Convert.ToInt32(_workContext.CurrentCustomer.Id.ToString());
            var customer = _customerService.GetCustomerById(customerId);

            _workContext.CurrentCustomer = customer;

            var cart = customer.ShoppingCartItems.Where(sci => sci.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();

            if (customer.BillingAddress == null)
            {
                // get/update billing address
                string name = stripeSource.Owner.Name;
                string billingEmail = customer.Email;
                string billingAddress1 = stripeSource.Owner.Address.Line1;
                string billingAddress2 = stripeSource.Owner.Address.Line2;
                string billingCity = stripeSource.Owner.Address.City;
                int? billingStateProvinceId = null;
                var billingStateProvince = _stateProvinceService.GetStateProvinceByAbbreviation(stripeSource.Owner.Address.State);
                if (billingStateProvince != null)
                    billingStateProvinceId = billingStateProvince.Id;
                string billingZipPostalCode = stripeSource.Owner.Address.PostalCode;
                int? billingCountryId = null;
                var billingCountry = _countryService.GetCountryByTwoLetterIsoCode(stripeSource.Owner.Address.Country);
                if (billingCountry != null)
                    billingCountryId = billingCountry.Id;

                var billingAddress = customer.Addresses.ToList().FindAddress(
                    null, null, null,
                    billingEmail, string.Empty, string.Empty, billingAddress1, billingAddress2, billingCity,
                    billingStateProvinceId, billingZipPostalCode, billingCountryId,
                    //TODO process custom attributes
                    null);

                billingAddress = new Core.Domain.Common.Address()
                {
                    FirstName = null,
                    LastName = null,
                    PhoneNumber = null,
                    Email = billingEmail,
                    FaxNumber = string.Empty,
                    Company = string.Empty,
                    Address1 = billingAddress1,
                    Address2 = billingAddress2,
                    City = billingCity,
                    StateProvinceId = billingStateProvinceId,
                    ZipPostalCode = billingZipPostalCode,
                    CountryId = billingCountryId,
                    CreatedOnUtc = DateTime.UtcNow,
                };
                customer.Addresses.Add(billingAddress);

                //set default billing address
                customer.BillingAddress = billingAddress;

                _customerService.UpdateCustomer(customer);
            }


            _genericAttributeService.SaveAttribute<ShippingOption>(customer, SystemCustomerAttributeNames.SelectedShippingOption, null);
                    
            var processPaymentRequest = new ProcessPaymentRequest { CustomerId = customerId };
            processPaymentRequest.CustomValues["StripeSourceId"] = stripeSource.Id;
            return processPaymentRequest;
        }
    }
}
