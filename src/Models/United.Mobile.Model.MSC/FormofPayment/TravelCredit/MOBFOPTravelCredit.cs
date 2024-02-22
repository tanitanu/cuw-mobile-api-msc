using System;
using System.Collections.Generic;

namespace United.Definition.FormofPayment.TravelCredit
{
    [Serializable()]
    public class MOBFOPTravelCredit
    {
        private string pinCode;
        private string yearIssued;
        private double redeemAmount;
        private double initialValue;
        private double currentValue;
        private bool isApplied;
        private bool isRemove = false;
        private string expiryDate;
        private string recordLocator;
        private string creditAmount;
        private double newValueAfterRedeem;
        private string displayRedeemAmount;
        private string displayNewValueAfterRedeem;
        private string promoCode;
        private MOBTravelCreditFOP travelCreditType;
        private string message = string.Empty;
        private List<string> eligibleTravelerNameIndex;
        private string recipient;
        private bool isLookupCredit;
        private List<string> eligibleTravelers;
        private string lastName;
        private string firstName;
        private bool isEligibleToRedeem;
        private bool isNameMatch;
        private bool isNameMatchWaiverApplied;
        private bool isTravelDateBeginsBeforeCertExpiry;
        private List<MOBTypeOption> captions;
        private bool isOTFFFC;
        private bool isAwardOTFEligible;
        private bool isHideShowDetails;
        private string isCorporateTravelCreditText;
        private string operationID = string.Empty;
        private string corporateName;
        private string certificateNumber = string.Empty;
        private United.Service.Presentation.CommonEnumModel.TravelCreditType csltravelCreditType;

        public United.Service.Presentation.CommonEnumModel.TravelCreditType CsltravelCreditType
        {
            get { return csltravelCreditType; }
            set { csltravelCreditType = value; }
        }
        public string CertificateNumber
        {
            get { return certificateNumber; }
            set { certificateNumber = value; }
        }
        public string OperationID
        {
            get { return operationID; }
            set { operationID = value; }
        }

        public bool IsHideShowDetails
        {
            get { return isHideShowDetails; }
            set { isHideShowDetails = value; }
        }
        public bool IsOTFFFC
        {
            get { return isOTFFFC; }
            set { isOTFFFC = value; }
        }
        public bool IsAwardOTFEligible
        {
            get { return isAwardOTFEligible; }
            set { isAwardOTFEligible = value; }
        }


        private int paxId;
        public int PaxId
        {
            get { return paxId; }
            set { paxId = value; }
        }

        private string travelerNameIndex;
        public string TravelerNameIndex
        {
            get { return travelerNameIndex; }
            set { travelerNameIndex = value; }
        }
        public bool IsEligibleToRedeem { get { return isEligibleToRedeem; } set { isEligibleToRedeem = value; } }
        public bool IsNameMatch { get { return isNameMatch; } set { isNameMatch = value; } }
        public bool IsNameMatchWaiverApplied { get { return isNameMatchWaiverApplied; } set { isNameMatchWaiverApplied = value; } }
        public bool IsTravelDateBeginsBeforeCertExpiry { get { return isTravelDateBeginsBeforeCertExpiry; } set { isTravelDateBeginsBeforeCertExpiry = value; } }
        public List<MOBTypeOption> Captions { get { return this.captions; } set { this.captions = value; } }

        public string LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }

        public string FirstName
        {
            get { return firstName; }
            set { firstName = value; }
        }

        public List<string> EligibleTravelers
        {
            get { return eligibleTravelers; }
            set { eligibleTravelers = value; }
        }


        public string Recipient
        {
            get { return recipient; }
            set { recipient = value; }
        }


        public List<string> EligibleTravelerNameIndex
        {
            get { return eligibleTravelerNameIndex; }
            set { eligibleTravelerNameIndex = value ?? new List<string>(); }
        }


        public string Message
        {
            get { return message; }
            set { message = value; }
        }


        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public MOBTravelCreditFOP TravelCreditType
        {
            get { return travelCreditType; }
            set { travelCreditType = value; }
        }

        public string PromoCode
        {
            get { return promoCode; }
            set { promoCode = value; }
        }

        public string CreditAmount
        {
            get { return creditAmount; }
            set { creditAmount = value; }
        }

        public string RecordLocator
        {
            get { return recordLocator; }
            set { recordLocator = value; }
        }

        public string ExpiryDate
        {
            get { return expiryDate; }
            set { expiryDate = value; }
        }

        public bool IsRemove
        {
            get { return isRemove; }
            set { isRemove = value; }
        }

        public bool IsApplied
        {
            get { return isApplied; }
            set { isApplied = value; }
        }

        public double NewValueAfterRedeem
        {
            get { return newValueAfterRedeem; }
            set { newValueAfterRedeem = value; }

        }

        public double RedeemAmount
        {
            get { return redeemAmount; }
            set { redeemAmount = value; }
        }

        public string DisplayRedeemAmount
        {
            get { return displayRedeemAmount; }
            set { displayRedeemAmount = value; }
        }

        public string DisplayNewValueAfterRedeem
        {
            get { return displayNewValueAfterRedeem; }
            set { displayNewValueAfterRedeem = value; }
        }

        public double CurrentValue
        {
            get { return currentValue; }
            set { currentValue = value; }
        }

        public double InitialValue
        {
            get { return initialValue; }
            set { initialValue = value; }
        }


        public string PinCode
        {
            get { return pinCode; }
            set { pinCode = value; }
        }

        public string YearIssued
        {
            get { return yearIssued; }
            set { yearIssued = value; }
        }

        public bool IsLookupCredit
        {
            get { return isLookupCredit; }
            set { isLookupCredit = value; }
        }

        public string IsCorporateTravelCreditText
        {
            get { return isCorporateTravelCreditText; }
            set { isCorporateTravelCreditText = value; }
        }
        public string CorporateName
        {
            get { return corporateName; }
            set { corporateName = value; }
        }
    }
}