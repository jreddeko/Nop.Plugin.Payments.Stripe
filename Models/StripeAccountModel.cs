using Stripe;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Nop.Plugin.Payments.Stripe.Models
{
    public class StripeAccountModel
    {
        public string Email { get; set; }

        [DisplayName("Account Id")]
        [Description("")]
        public string AccountId { get; set; }

        public List<string> Issues { get; set; }

        public int DateOfBirthDay { get; set; }
        public int DateOfBirthMonth { get; set; }
        public int DateOfBirthYear { get; set; }

        [DisplayName("SIN")]
        [Description("The ID number of the representative, as appropriate for the legal entity’s country. For example, a social insurance number in Canada")]
        public string PiiToken { get; set; }

        [DisplayName("First Name")]
        [Description("The first name of the representative of this legal entity")]
        public string FirstName { get; set; }

        [DisplayName("Last Name")]
        [Description("The last name of the representative of this legal entity")]
        public string LastName { get; set; }

        [DisplayName("Maiden Name")]
        [Description("The maiden name of the individual responsible for the account")]
        public string MaidenName { get; set; }

        [DisplayName("Personal Address")]
        [Description("The personal address of the representative of this legal entity, used for verification")]
        public StripeAddress PersonalAddress { get; set; }

        [DisplayName("Company Address")]
        [Description("The primary address of the legal entity")]
        public StripeAddress CompanyAddress { get; set; }

        [Description("A card or bank account to attach to the account. ")]
        public string BankAccountToken { get; set; }

        [DisplayName("Business Name")]
        [Description("The legal name of the company")]
        public string BusinessName { get; set; }

        [DisplayName("Business Number")]
        [Description("The business number of the legal entity, if it’s registered as a company, as appropriate for the country it’s in.")]
        public string BusinessTaxId { get; set; }

        [DisplayName("Phone Number")]
        [Description("The phone number of the company, used for verification")]

        public string PhoneNumber { get; set; }

        [DisplayName("Type")]
        [Description("The legal entity the account owner is for")]
        public string Type { get; set; }
        public ICollection<SelectListItem> AvailableCountries { get; internal set; }
        public ICollection<SelectListItem> AvailableStates { get; internal set; }

        public StripeAccountModel()
        {
            this.CompanyAddress = new StripeAddress();
            this.PersonalAddress = new StripeAddress();
            this.AvailableCountries = new List<SelectListItem>();
            this.AvailableStates = new List<SelectListItem>();
        }
    }
}
