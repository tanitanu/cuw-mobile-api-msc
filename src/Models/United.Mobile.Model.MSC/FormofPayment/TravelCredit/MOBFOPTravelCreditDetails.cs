using United.Definition.Shopping;
using System;
using System.Collections.Generic;


namespace United.Definition.FormofPayment.TravelCredit
{
    [Serializable()]
    public class MOBFOPTravelCreditDetails
    {
		
        private List<MOBMobileCMSContentMessages> lookUpMessages;
        private List<MOBMobileCMSContentMessages> alertMessages;
        private List<MOBMobileCMSContentMessages> reviewMessages;
        private List<MOBFOPTravelCredit> travelCredits;
        private MOBSection findETCConfirmationMessage;

        private double totalRedeemAmount;
        private string nameWaiverMatchMessage;

        private string travelCreditSummary;
        private string corporateName;

        public string NameWaiverMatchMessage
        {
            get { return nameWaiverMatchMessage; }
            set { nameWaiverMatchMessage = value; }
        }

        public List<MOBMobileCMSContentMessages> LookUpMessages
        {
			get { return lookUpMessages; }
			set { lookUpMessages = value; }
		}


        public List<MOBMobileCMSContentMessages> AlertMessages
        {
            get { return alertMessages; }
            set { alertMessages = value; }
        }

        public List<MOBMobileCMSContentMessages> ReviewMessages
        {
            get { return reviewMessages; }
            set { reviewMessages = value; }
        }

        public List<MOBFOPTravelCredit> TravelCredits
        {
            get { return travelCredits; }
            set { travelCredits = value; }
        }
        public MOBSection FindETCConfirmationMessage
        {
            get { return findETCConfirmationMessage; }
            set { findETCConfirmationMessage = value; }
        }
        public double TotalRedeemAmount
        {
            get
            {
                totalRedeemAmount = 0;
                if (travelCredits != null && travelCredits.Count > 0)
                {
                    foreach (var travelCredit in travelCredits)
                    {
                        if(travelCredit.IsApplied)
                            totalRedeemAmount += travelCredit.RedeemAmount;
                    }
                }
                return totalRedeemAmount;
            }
        }

        public string TravelCreditSummary
        {
            get { return travelCreditSummary; }
            set { travelCreditSummary = value; }
        }
        public string CorporateName
        {
            get { return corporateName; }
            set { corporateName = value; }
        }
    }
}
