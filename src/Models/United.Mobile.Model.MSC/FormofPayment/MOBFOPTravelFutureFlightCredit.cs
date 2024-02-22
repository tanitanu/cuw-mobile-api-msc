﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;
using United.Definition.Shopping;

namespace United.Definition.FormofPayment
{
    [Serializable()]
    public class MOBFOPTravelFutureFlightCredit
    {

        private string ffcButtonText;
        private double totalRedeemAmount;
        private string displayTotalRedeemAmountText;
        private List<MOBMobileCMSContentMessages> learnmoreTermsandConditions;
        private List<MOBMobileCMSContentMessages> reviewFFCMessages;
        private List<MOBMobileCMSContentMessages> lookUpFFCMessages;
        private List<MOBMobileCMSContentMessages> findFFCMessages;
        private MOBSection findFFCConfirmationMessage;
        private List<MOBMobileCMSContentMessages> emailConfirmationFFCMessages;
        private MOBSection goToTripDetailDialog;

        public MOBSection GoToTripDetailDialog
        {
            get { return goToTripDetailDialog; }
            set { goToTripDetailDialog = value; }
        }


        public List<MOBMobileCMSContentMessages> EmailConfirmationFFCMessages
        {
            get { return emailConfirmationFFCMessages; }
            set { emailConfirmationFFCMessages = value; }
        }


        private List<MOBFOPFutureFlightCredit> futureFlightCredits;
        public List<MOBFOPFutureFlightCredit> FutureFlightCredits
        {
            get { return futureFlightCredits; }
            set { futureFlightCredits = value; }
        }


        public MOBSection FindFFCConfirmationMessage
        {
            get { return findFFCConfirmationMessage; }
            set { findFFCConfirmationMessage = value; }
        }


        public List<MOBMobileCMSContentMessages> FindFFCMessages
        {
            get { return findFFCMessages; }
            set { findFFCMessages = value; }
        }

        //private MOBSection removeAllCertificateAlertMessage;

        public List<MOBMobileCMSContentMessages> LookUpFFCMessages
        {
            get { return lookUpFFCMessages; }
            set { lookUpFFCMessages = value; }
        }

        public List<MOBMobileCMSContentMessages> ReviewFFCMessages
        {
            get { return reviewFFCMessages; }
            set { reviewFFCMessages = value; }
        }

        public List<MOBMobileCMSContentMessages> LearnmoreTermsandConditions
        {
            get
            {
                return this.learnmoreTermsandConditions;
            }
            set
            {
                this.learnmoreTermsandConditions = value;
            }
        }


        public string DisplayTotalRedeemAmountText
        {
            get
            {
                displayTotalRedeemAmountText = (TotalRedeemAmount).ToString("N2", CultureInfo.CurrentCulture);
                return displayTotalRedeemAmountText;
            }

        }

        //public List<MOBFOPFutureFlightCredit> FutureFlightCredits
        //{
        //    get { return futureFlightCredits; }
        //    set { futureFlightCredits = value; }
        //}

        public string FFCButtonText
        {
            get { return ffcButtonText; }
            set { ffcButtonText = value; }
        }

        public double TotalRedeemAmount
        {
            get
            {
                totalRedeemAmount = 0;
                if (futureFlightCredits != null && futureFlightCredits.Count > 0)
                {
                    foreach (var certificate in futureFlightCredits)
                    {
                        totalRedeemAmount += certificate.RedeemAmount;
                    }
                }
                return Math.Round(totalRedeemAmount, 2, MidpointRounding.AwayFromZero);
            }
        }
        //public MOBSection RemoveAllCertificateAlertMessage
        //{
        //    get { return removeAllCertificateAlertMessage; }
        //    set { removeAllCertificateAlertMessage = value; }
        //}
        private double allowedFFCAmount;
        public double AllowedFFCAmount
        {
            get { return allowedFFCAmount; }
            set { allowedFFCAmount = value; }
        }

        private double totalAllowedAncillaryAmount;

        [JsonPropertyName("totalAllowedAncillaryAmount")]
        [JsonProperty("totalAllowedAncillaryAmount")]
        public double AllowedAncillaryAmount
        {
            get { return totalAllowedAncillaryAmount; }
            set { totalAllowedAncillaryAmount = value; }
        }

    }
}
