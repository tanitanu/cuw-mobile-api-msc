﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace United.Definition
{
    [Serializable()]
    public class MOBMPAccountSummary
    {
        private string mileagePlusNumber = string.Empty;
        private long customerId;
        private MOBName name;
        private string balance = "0";
        private string balanceExpireDate = string.Empty;
        private string balanceExpireDisclaimer = string.Empty;
        private string noMileageExpiration = string.Empty;
        private string noMileageExpirationMessage = string.Empty;
        private MOBEliteStatus eliteStatus;
        private string enrollDate = string.Empty;
        private string lastFlightDate = string.Empty;
        private string lastActivityDate = string.Empty;
        public string eliteMileage = "0";
        public string eliteSegment = "0";

        private string lastExpiredMileDate = string.Empty;
        private int lastExpiredMile = 0;

        private bool hasUAClubMemberShip;
        private MOBUnitedClubMemberShipDetails uAClubMemberShipDetails;
        private bool isMPAccountTSAFlagON;
        private string tsaMessage = string.Empty;

        private string fourSegmentMinimun = string.Empty;
        private string premierQualifyingDollars = string.Empty;
        private string pDQchasewavier = string.Empty;
        private string pDQchasewaiverLabel = string.Empty;
        private string millionMilerIndicator = string.Empty;

        private byte[] membershipCardBarCode;
        private string membershipCardBarCodeString;
        private bool isCEO;
        private string hashValue;

        private string membershipCardExpirationDate  = string.Empty;

        private bool showChaseBonusTile;
        private int lifetimeMiles;

        public MOBMPAccountSummary()
        {
        }

        public string MileagePlusNumber
        {
            get
            {
                return this.mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        [XmlIgnore]
        public long CustomerId
        {
            get
            {
                return this.customerId;
            }
            set
            {
                this.customerId = value;
            }
        }

        public MOBName Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public string Balance
        {
            get
            {
                //The below commented changes made by Naresh to fix the mileage Plus balance summary(Mobile Web QC# 914) has broken iPhone and Android display issue and caused Android to crash - Venkat 04/17/2012
                //if (!String.IsNullOrEmpty(this.balance)) {
                //    int bal = int.Parse(balance);
                //    if (bal > 0) {
                //        string formattedBal = bal.ToString("#,#");
                //        return formattedBal;
                //    }
                //    return this.balance;
                //}
                return this.balance;
            }
            set
            {
                this.balance = string.IsNullOrEmpty(value) ? "0" : value.Trim();
            }
        }


        public string FormattedBalance
        {
            get
            {
                if (!String.IsNullOrEmpty(this.balance))
                {
                    int bal = int.Parse(balance);
                    if (bal > 0)
                    {
                        string formattedBal = bal.ToString("#,#");
                        return formattedBal;
                    }
                    return this.balance;
                }
                return String.Empty;
            }
        }

        public string BalanceExpireDate
        {
            get
            {
                return this.balanceExpireDate;
            }
            set
            {
                this.balanceExpireDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string BalanceExpireDisclaimer
        {
            get
            {
                return this.balanceExpireDisclaimer;
            }
            set
            {
                this.balanceExpireDisclaimer = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string NoMileageExpiration
        {
            get
            {
                return this.noMileageExpiration;
            }
            set
            {
                this.noMileageExpiration = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string NoMileageExpirationMessage
        {
            get
            {
                return this.noMileageExpirationMessage;
            }
            set
            {
                this.noMileageExpirationMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBEliteStatus EliteStatus
        {
            get
            {
                return this.eliteStatus;
            }
            set
            {
                this.eliteStatus = value;
            }
        }

        public string EnrollDate
        {
            get
            {
                return this.enrollDate;
            }
            set
            {
                this.enrollDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string LastFlightDate
        {
            get
            {
                return this.lastFlightDate;
            }
            set
            {
                this.lastFlightDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string LastActivityDate
        {
            get
            {
                return this.lastActivityDate;
            }
            set
            {
                this.lastActivityDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string EliteMileage
        {
            get
            {
                return this.eliteMileage;
            }
            set
            {
                this.eliteMileage = string.IsNullOrEmpty(value) ? "0" : value.Trim();
            }
        }

        public string EliteSegment
        {
            get
            {
                return this.eliteSegment;
            }
            set
            {
                this.eliteSegment = string.IsNullOrEmpty(value) ? "0" : value.Trim();
            }
        }

        public string LastExpiredMileDate
        {
            get
            {
                return this.lastExpiredMileDate;
            }
            set
            {
                this.lastExpiredMileDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int LastExpiredMile
        {
            get
            {
                return this.lastExpiredMile;
            }
            set
            {
                this.lastExpiredMile = value;
            }
        }

        public MOBUnitedClubMemberShipDetails UAClubMemberShipDetails
        {
            get
            {
                return this.uAClubMemberShipDetails;
            }
            set
            {
                this.uAClubMemberShipDetails = value;
            }
        }

        public bool HasUAClubMemberShip
        {
            get
            {
                return this.hasUAClubMemberShip;
            }
            set
            {
                this.hasUAClubMemberShip = value;
            }
        }

        public bool IsMPAccountTSAFlagON
        {
            get
            {
                return this.isMPAccountTSAFlagON;
            }
            set
            {
                this.isMPAccountTSAFlagON = value;
            }
        }

        public string TSAMessage
        {
            get
            {
                return this.tsaMessage;
            }
            set
            {
                this.tsaMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FourSegmentMinimun
        {
            get
            {
                return this.fourSegmentMinimun;
            }
            set
            {
                this.fourSegmentMinimun = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string PremierQualifyingDollars
        {
            get
            {
                return this.premierQualifyingDollars;
            }
            set
            {
                this.premierQualifyingDollars = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string PDQchasewavier
        {
            get
            {
                return this.pDQchasewavier;
            }
            set
            {
                this.pDQchasewavier = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string PDQchasewaiverLabel
        {
            get
            {
                return this.pDQchasewaiverLabel;
            }
            set
            {
                this.pDQchasewaiverLabel = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string MillionMilerIndicator
        {
            get
            {
                return this.millionMilerIndicator;
            }
            set
            {
                this.millionMilerIndicator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public byte[] MembershipCardBarCode
        {
            get
            {
                return this.membershipCardBarCode;
            }
            set
            {
                this.membershipCardBarCode = value;
            }
        }

        public string MembershipCardBarCodeString
        {
            get
            {
                return this.membershipCardBarCodeString;
            }
            set
            {
                this.membershipCardBarCodeString = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool IsCEO
        {
            get
            {
                return this.isCEO;
            }
            set
            {
                this.isCEO = value;
            }
        }

        public string HashValue
        {
            get
            {
                return this.hashValue;
            }
            set
            {
                this.hashValue = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string MembershipCardExpirationDate
        {
            get
            {
                return this.membershipCardExpirationDate;
            }
            set
            {
                this.membershipCardExpirationDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool ShowChaseBonusTile
        {
            get
            {
                return this.showChaseBonusTile;
            }
            set
            {
                this.showChaseBonusTile = value;
            }
        }

        public int LifetimeMiles
        {
            get
            {
                return this.lifetimeMiles;
            }
            set
            {
                this.lifetimeMiles = value;
            }
        }

        private string chasePromoType;
        public string ChasePromoType
        {
            get { return chasePromoType; }
            set { chasePromoType = value; }
        }

        //Start - 221924 - Added by Nizam on 12/19/2017
        private MOBMPStatusLiftBanner statusLiftBanner;

        public MOBMPStatusLiftBanner StatusLiftBanner
        {
            get { return statusLiftBanner; }
            set { statusLiftBanner = value; }
        }
        //End - 221924 - Added by Nizam on 12/19/2017
    }

    //Start - 221924 - Added by Nizam on 12/19/2017
    public class MOBMPStatusLiftBanner
    {
        private string imageSrcURL;
        private string premierStatusURL;

        public string ImageSrcURL
        {
            get
            {
                return this.imageSrcURL;
            }
            set
            {
                this.imageSrcURL = value;
            }
        }

        public string PremierStatusURL
        {
            get
            {
                return this.premierStatusURL;
            }
            set
            {
                this.premierStatusURL = value;
            }
        }
    }
    //End - 221924 - Added by Nizam on 12/19/2017
}
