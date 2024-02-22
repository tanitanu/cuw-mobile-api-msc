﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MOBCPMileagePlus
    {
        private int accountBalance;
        private string activeStatusCode = string.Empty;
        private string activeStatusDescription = string.Empty;
        private int allianceEliteLevel;
        private string closedStatusCode = string.Empty;
        private string closedStatusDescription = string.Empty;
        private int currentEliteLevel;
        private string currentEliteLevelDescription = string.Empty;
        private decimal currentYearMoneySpent;
        private int eliteMileageBalance;
        private int eliteSegmentBalance;
        private int eliteSegmentDecimalPlaceValue;
        private string encryptedPin = string.Empty;
        private string enrollDate = string.Empty;
        private string enrollSourceCode = string.Empty;
        private string enrollSourceDescription = string.Empty;
        private int flexPqmBalance;
        private int futureEliteLevel;
        private string futureEliteDescription = string.Empty;
        private string instantEliteExpirationDate = string.Empty;
        private bool isCEO;
        private bool isClosedPermanently;
        private bool isClosedTemporarily;
        private bool isCurrentTrialEliteMember;
        private bool isFlexPqm;
        private bool isInfiniteElite;
        private bool isLifetimeCompanion;
        private bool isLockedOut;
        private bool isMergePending;
        private bool isUnitedClubMember;
        private bool isPresidentialPlus;
        private string key = string.Empty;
        private string lastActivityDate = string.Empty;
        private int lastExpiredMile;
        private string lastFlightDate = string.Empty;
        private int lastStatementBalance;
        private string lastStatementDate = string.Empty;
        private int lifetimeEliteLevel;
        private int lifetimeEliteMileageBalance;
        private string mileageExpirationDate = string.Empty;
        private int nextYearEliteLevel;
        private string nextYearEliteLevelDescription = string.Empty;
        private string mileagePlusId = string.Empty;
        private string mileagePlusPin = string.Empty;
        private string priorUnitedAccountNumber = string.Empty;
        private int starAllianceEliteLevel;
        private int mpCustomerId;
        private string vendorCode;
        private string travelBankAccountNumber;
        private double travelBankBalance;
        private string travelBankCurrencyCode;
        private string travelBankExpirationDate;
        private string premierLevelExpirationDate = string.Empty;
        private MOBInstantElite instantElite;


        public MOBInstantElite InstantElite
        {
            get
            {
                return this.instantElite;
            }
            set
            {
                this.instantElite = value;
            }
        }
        public int AccountBalance
        {
            get
            {
                return this.accountBalance;
            }
            set
            {
                this.accountBalance = value;
            }
        }

        public string ActiveStatusCode
        {
            get
            {
                return this.activeStatusCode;
            }
            set
            {
                this.activeStatusCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ActiveStatusDescription
        {
            get
            {
                return this.activeStatusDescription;
            }
            set
            {
                this.activeStatusDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int AllianceEliteLevel
        {
            get
            {
                return this.allianceEliteLevel;
            }
            set
            {
                this.allianceEliteLevel = value;
            }
        }

        public string ClosedStatusCode
        {
            get
            {
                return this.closedStatusCode;
            }
            set
            {
                this.closedStatusCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ClosedStatusDescription
        {
            get
            {
                return this.closedStatusDescription;
            }
            set
            {
                this.closedStatusDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int CurrentEliteLevel
        {
            get
            {
                return this.currentEliteLevel;
            }
            set
            {
                this.currentEliteLevel = value;
            }
        }

        public string CurrentEliteLevelDescription
        {
            get
            {
                return this.currentEliteLevelDescription;
            }
            set
            {
                this.currentEliteLevelDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public decimal CurrentYearMoneySpent
        {
            get
            {
                return this.currentYearMoneySpent;
            }
            set
            {
                this.currentYearMoneySpent = value;
            }
        }

        public int EliteMileageBalance
        {
            get
            {
                return this.eliteMileageBalance;
            }
            set
            {
                this.eliteMileageBalance = value;
            }
        }

        public int EliteSegmentBalance
        {
            get
            {
                return this.eliteSegmentBalance;
            }
            set
            {
                this.eliteSegmentBalance = value;
            }
        }

        public int EliteSegmentDecimalPlaceValue
        {
            get
            {
                return this.eliteSegmentDecimalPlaceValue;
            }
            set
            {
                this.eliteSegmentDecimalPlaceValue = value;
            }
        }

        public string EncryptedPin
        {
            get
            {
                return this.encryptedPin;
            }
            set
            {
                this.encryptedPin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        public string EnrollSourceCode
        {
            get
            {
                return this.enrollSourceCode;
            }
            set
            {
                this.enrollSourceCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string EnrollSourceDescription
        {
            get
            {
                return this.enrollSourceDescription;
            }
            set
            {
                this.enrollSourceDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int FlexPqmBalance
        {
            get
            {
                return this.flexPqmBalance;
            }
            set
            {
                this.flexPqmBalance = value;
            }
        }

        public int FutureEliteLevel
        {
            get
            {
                return this.futureEliteLevel;
            }
            set
            {
                this.futureEliteLevel = value;
            }
        }

        public string FutureEliteDescription
        {
            get
            {
                return this.futureEliteDescription;
            }
            set
            {
                this.futureEliteDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string InstantEliteExpirationDate
        {
            get
            {
                return this.instantEliteExpirationDate;
            }
            set
            {
                this.instantEliteExpirationDate = string.IsNullOrEmpty(value) ? string.Empty: value.Trim();
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

        public bool IsClosedPermanently
        {
            get
            {
                return this.isClosedPermanently;
            }
            set
            {
                this.isClosedPermanently = value;
            }
        }

        public bool IsClosedTemporarily
        {
            get
            {
                return this.isClosedTemporarily;
            }
            set
            {
                this.isClosedTemporarily = value;
            }
        }

        public bool IsCurrentTrialEliteMember
        {
            get
            {
                return this.isCurrentTrialEliteMember;
            }
            set
            {
                this.isCurrentTrialEliteMember = value;
            }
        }

        public bool IsFlexPqm
        {
            get
            {
                return this.isFlexPqm;
            }
            set
            {
                this.isFlexPqm = value;
            }
        }

        public bool IsInfiniteElite
        {
            get
            {
                return this.isInfiniteElite;
            }
            set
            {
                this.isInfiniteElite = value;
            }
        }

        public bool IsLifetimeCompanion
        {
            get
            {
                return this.isLifetimeCompanion;
            }
            set
            {
                this.isLifetimeCompanion = value;
            }
        }

        public bool IsLockedOut
        {
            get
            {
                return this.isLockedOut;
            }
            set
            {
                this.isLockedOut = value;
            }
        }

        public bool IsMergePending
        {
            get
            {
                return this.isMergePending;
            }
            set
            {
                this.isMergePending = value;
            }
        }

        public bool IsUnitedClubMember
        {
            get
            {
                return this.isUnitedClubMember;
            }
            set
            {
                this.isUnitedClubMember = value;
            }
        }

        public bool IsPresidentialPlus
        {
            get
            {
                return this.isPresidentialPlus;
            }
            set
            {
                this.isPresidentialPlus = value;
            }
        }

        public string Key
        {
            get
            {
                return this.key;
            }
            set
            {
                this.key = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        public int LastStatementBalance
        {
            get
            {
                return this.lastStatementBalance;
            }
            set
            {
                this.lastStatementBalance = value;
            }
        }

        public string LastStatementDate
        {
            get
            {
                return this.lastStatementDate;
            }
            set
            {
                this.lastStatementDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int LifetimeEliteLevel
        {
            get
            {
                return this.lifetimeEliteLevel;
            }
            set
            {
                this.lifetimeEliteLevel = value;
            }
        }

        public int LifetimeEliteMileageBalance
        {
            get
            {
                return this.lifetimeEliteMileageBalance;
            }
            set
            {
                this.lifetimeEliteMileageBalance = value;
            }
        }

        public string MileageExpirationDate
        {
            get
            {
                return this.mileageExpirationDate;
            }
            set
            {
                this.mileageExpirationDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int NextYearEliteLevel
        {
            get
            {
                return this.nextYearEliteLevel;
            }
            set
            {
                this.nextYearEliteLevel = value;
            }
        }

        public string NextYearEliteLevelDescription
        {
            get
            {
                return this.nextYearEliteLevelDescription;
            }
            set
            {
                this.nextYearEliteLevelDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string MileagePlusId
        {
            get
            {
                return this.mileagePlusId;
            }
            set
            {
                this.mileagePlusId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string MileagePlusPin
        {
            get
            {
                return this.mileagePlusPin;
            }
            set
            {
                this.mileagePlusPin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string PriorUnitedAccountNumber
        {
            get
            {
                return this.priorUnitedAccountNumber;
            }
            set
            {
                this.priorUnitedAccountNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int StarAllianceEliteLevel
        {
            get
            {
                return this.starAllianceEliteLevel;
            }
            set
            {
                this.starAllianceEliteLevel = value;
            }
        }

        public int MpCustomerId
        {
            get
            {
                return this.mpCustomerId;
            }
            set
            {
                this.mpCustomerId = value;
            }
        }

        public string VendorCode
        {
            get { return vendorCode; }
            set { vendorCode = value; }
        }

        public string TravelBankAccountNumber
        {
            get { return travelBankAccountNumber; }
            set { travelBankAccountNumber = value; }
        }

        public double TravelBankBalance { get => travelBankBalance; set => travelBankBalance = value; }
        public string TravelBankCurrencyCode
        {
            get { return travelBankCurrencyCode; }
            set { travelBankCurrencyCode = value; }
        }
        public string TravelBankExpirationDate 
        {
            get { return travelBankExpirationDate; }
            set { travelBankExpirationDate = value; }
        }
        public string PremierLevelExpirationDate
        {
            get
            {
                return this.premierLevelExpirationDate;
            }
            set
            {
                this.premierLevelExpirationDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }

    [Serializable()]
    public class MOBInstantElite
    {
        private string consolidatedCode = string.Empty;
        private string effectiveDate = string.Empty;
        private int eliteLevel;
        private int eliteYear;
        private string expirationDate = string.Empty;
        private string promotionCode = string.Empty;



        public int EliteLevel
        {
            get
            {
                return this.eliteLevel;
            }
            set
            {
                this.eliteLevel = value;
            }
        }

        public int EliteYear
        {
            get
            {
                return this.eliteYear;
            }
            set
            {
                this.eliteYear = value;
            }
        }

        public string ConsolidatedCode
        {
            get
            {
                return this.consolidatedCode;
            }
            set
            {
                this.consolidatedCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string EffectiveDate
        {
            get
            {
                return this.effectiveDate;
            }
            set
            {
                this.effectiveDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ExpirationDate
        {
            get
            {
                return this.expirationDate;
            }
            set
            {
                this.expirationDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string PromotionCode
        {
            get
            {
                return this.promotionCode;
            }
            set
            {
                this.promotionCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
