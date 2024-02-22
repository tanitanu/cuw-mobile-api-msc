using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;
using United.Mobile.Model.Common;

namespace United.Definition
{
    [Serializable()]
    public class MOBProvisionAccountDetails : MOBResponse
    {
        private MOBCreditCard selectedCreditCard;
        private string provisionRequestIdentifier;
        private List<MOBMobileCMSContentMessages> mobMobileCMSContentMessages;
        private bool isExistCreditCard = false;
        private string provisionSuccessMessage;
        private MOBAddress selectedAddress;
        private string accountReferenceIdentifier;
        private string partnerRequestIdentifier;

        public MOBCreditCard SelectedCreditCard
        {
            get { return this.selectedCreditCard; }
            set { this.selectedCreditCard = value; }
        }
        public string ProvisionRequestIdentifier 
        { 
            get { return this.provisionRequestIdentifier; } 
            set { this.provisionRequestIdentifier = value; } 
        }
        public List<MOBMobileCMSContentMessages> MOBMobileCMSContentMessages
        {
            get { return this.mobMobileCMSContentMessages; }
            set { this.mobMobileCMSContentMessages = value; }
        }
        public bool IsExistCreditCard
        {
            get { return this.isExistCreditCard; }
            set { this.isExistCreditCard = value; }
        }
        public string ProvisionSuccessMessage
        {
            get { return this.provisionSuccessMessage; }
            set { this.provisionSuccessMessage = value; }
        }
        public MOBAddress SelectedAddress
        {
            set { selectedAddress = value; }
            get { return selectedAddress; }
        }
        public string AccountReferenceIdentifier
        {
            get { return this.accountReferenceIdentifier; }
            set { this.accountReferenceIdentifier = value; }
        }
        public string PartnerRequestIdentifier
        {
            get { return partnerRequestIdentifier; }
            set { this.partnerRequestIdentifier = value; }
        }
    }

    public class MOBGetProvisionDetailsResponse
    {
        private AccountInfo accountInfo;
        private AddressInfo addressInfo;
        private List<Errors> errors;

        public AddressInfo AddressInfo { get { return this.addressInfo; } set { this.addressInfo = value; } }
        public AccountInfo AccountInfo { get { return this.accountInfo; } set { this.accountInfo = value; } }
        public List<Errors> Errors { get { return this.errors; } set { this.errors = value; } }

    }

    public class AccountInfo
    {
        public string AccountReferenceIdentifier { get; set; }
        public string AccountNumberToken { get; set; }
        public string PersistentToken { get; set; }
        public string AccountCode { get; set; }
        public string ExpirationMonth { get; set; }
        public string ExpirationYear { get; set; }
        public string CardTypeName { get; set; }
        public string CardTypeCode { get; set; }
        public string CardType { get; set; }
    }

    public class AddressInfo
    {
        public string FullName { get; set; }
        public string AddressKey { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressCityName { get; set; }
        public string AddressStateCode { get; set; }
        public string AddressCountryCode { get; set; }
        public string AddressPostalCode { get; set; }
    }

    public class Errors
    {
        public string ApplicationName { get; set; }
        public string Code { get; set; }
        public string MajorCode { get; set; }
        public string MajorDescription { get; set; }
        public string MinorCode { get; set; }
        public string MinorDescription { get; set; }
        public string Severity { get; set; }
    }
    
}
