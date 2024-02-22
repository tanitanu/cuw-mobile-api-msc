﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable]
    public class MOBUpgradeListResponse : MOBResponse
    {
        private string flightNumber = string.Empty;
        private string flightDate = string.Empty;
        private string departureAirportCode = string.Empty;
        private MOBUpgradeList upgradeList;
        private MOBDoDOfferEligibilityInfo eligibilityInfoForDoDOffer;

        public MOBUpgradeListResponse()
            : base()
        {
        }

        public string FlightNumber
        {
            get
            {
                return this.flightNumber;
            }
            set
            {
                this.flightNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FlightDate
        {
            get
            {
                return this.flightDate;
            }
            set
            {
                this.flightDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string DepartureAirportCode
        {
            get
            {
                return this.departureAirportCode;
            }
            set
            {
                this.departureAirportCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public MOBUpgradeList UpgradeList
        {
            get
            {
                return this.upgradeList;
            }
            set
            {
                this.upgradeList = value;
            }
        }

        public MOBDoDOfferEligibilityInfo EligibilityInfoForDoDOffer
        {
            get { return eligibilityInfoForDoDOffer; }
            set { eligibilityInfoForDoDOffer = value; }
        }
    }
}
