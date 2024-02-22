﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.SSR;
using United.Mobile.Model.Common;
using United.Service.Presentation.CommonModel;

namespace United.Definition
{
    [Serializable()]
    public class MOBPNRPassenger
    {
        private MOBName passengerName;       
        private string knownTravelerNumber;
        private string ktnDisplaySequence;
        private string redressNumber;
        private string redressDisplaySequence;
        private string ssrDisplaySequence;
        private string sharesPosition = string.Empty;
        private MOBCPMileagePlus mileagePlus;
        private List<MOBRewardProgram> oaRewardPrograms;
        private bool isMPMember;
        private string sharesGivenName = string.Empty;
        private List<MOBTravelSpecialNeed> selectedSpecialNeeds;
        private string birthDate = string.Empty;
        private string travelerTypeCode = string.Empty;
        private string canadianTravelNumber;
        private MOBContact contact;
        private string ctnDisplaySequence;
        private MOBLMXTraveler earnedMiles;
        private MOBPNREmployeeProfile employeeProfile;
        private LoyaltyProgramProfile loyaltyProgramProfile;
        private string pnrCustomerID;
        private string pricingPaxType = string.Empty;

        public string PricingPaxType
        {
            get
            { return this.pricingPaxType; }
            set
            { this.pricingPaxType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        public string PNRCustomerID
        {
            get { return this.pnrCustomerID; }
            set { this.pnrCustomerID = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public LoyaltyProgramProfile LoyaltyProgramProfile
        {
            get
            {
                return this.loyaltyProgramProfile;
            }
            set
            {
                this.loyaltyProgramProfile = value;
            }
        }

        public MOBPNREmployeeProfile EmployeeProfile
        {
            get
            {
                return this.employeeProfile;
            }
            set
            {
                this.employeeProfile = value;
            }
        }

        public MOBLMXTraveler EarnedMiles
        { get { return this.earnedMiles; } set { this.earnedMiles = value; } }

        public string CTNDisplaySequence
        {
            get { return this.ctnDisplaySequence; }
            set { this.ctnDisplaySequence = string.IsNullOrWhiteSpace(value) ? string.Empty : value; }
        }

        public MOBContact Contact
        {
            get { return this.contact; }
            set { this.contact = value; }
        }

        public string CanadianTravelNumber { get { return this.canadianTravelNumber; } set { this.canadianTravelNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); } }
        public string BirthDate
        {
            get
            { return this.birthDate; }
            set
            {this.birthDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();}
        }

        public string TravelerTypeCode
        {
            get
            { return this.travelerTypeCode; }
            set
            { this.travelerTypeCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }       

        public string SSRDisplaySequence
        {
            get { return this.ssrDisplaySequence; }
            set { this.ssrDisplaySequence = string.IsNullOrWhiteSpace(value) ? string.Empty : value; }
        }

        public List<MOBTravelSpecialNeed> SelectedSpecialNeeds
        {
            get {return this.selectedSpecialNeeds; }
            set { this.selectedSpecialNeeds = value; }
        }

        public string SharesGivenName
        {
            get { return this.sharesGivenName; }
            set { this.sharesGivenName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public string KnownTravelerNumber { get { return this.knownTravelerNumber; } set {this.knownTravelerNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); } }

        public string RedressNumber { get { return this.redressNumber; } set { this.redressNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); } }

        public string KTNDisplaySequence { get { return this.ktnDisplaySequence; } set { this.ktnDisplaySequence = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); } }

        public string REDRESSDisplaySequence { get { return this.redressDisplaySequence; } set { this.redressDisplaySequence = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); } }


        public MOBName PassengerName
        {
            get
            {
                return this.passengerName;
            }
            set
            {
                this.passengerName = value;
            }
        }

        public string SHARESPosition
        {
            get
            {
                return this.sharesPosition;
            }
            set
            {
                this.sharesPosition = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBCPMileagePlus MileagePlus
        {
            get
            {
                return this.mileagePlus;
            }
            set
            {
                this.mileagePlus = value;
            }
        }

        public List<MOBRewardProgram> OaRewardPrograms
        {
            get
            {
                return this.oaRewardPrograms;
            }
            set
            {
                this.oaRewardPrograms = value;
            }
        }

        public bool IsMPMember
        {
            get
            {
                return this.isMPMember;
            }
            set
            {
                this.isMPMember = value;
            }
        }
    }
}
