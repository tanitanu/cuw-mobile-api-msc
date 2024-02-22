using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MOBShopPricesCommon
    {
        private decimal brokenOutYQSurcharges;
        private decimal businessAirfare;
        private decimal cheapestAirfare;
        private decimal cheapestAirfareNoConnections;
        private decimal cheapestAirfareWithConnections;
        private decimal cheapestAirfareWithConnNonPartner;
        private decimal cheapestAirfareWithConnPartner;
        private decimal cheapestAltDate;
        private decimal cheapestNearbyAirport;
        private decimal cheapestRefundable;
        private decimal cheapestWithoutOffer;
        private decimal firstClassAirfare;
        private string firstClassAirfareNotShownReason = string.Empty;
        private decimal fullYAirfare;
        private decimal mostExpensiveAirfare;
        private decimal refundableAirfare;
        private decimal refundableAverageAirfarePerPax;

        public decimal BrokenOutYQSurcharges
        {
            get
            {
                return this.brokenOutYQSurcharges;
            }
            set
            {
                this.brokenOutYQSurcharges = value;
            }
        }

        public decimal BusinessAirfare
        {
            get
            {
                return this.businessAirfare;
            }
            set
            {
                this.businessAirfare = value;
            }
        }

        public decimal CheapestAirfare
        {
            get
            {
                return this.cheapestAirfare;
            }
            set
            {
                this.cheapestAirfare = value;
            }
        }

        public decimal CheapestAirfareNoConnections
        {
            get
            {
                return this.cheapestAirfareNoConnections;
            }
            set
            {
                this.cheapestAirfareNoConnections = value;
            }
        }

        public decimal CheapestAirfareWithConnections
        {
            get
            {
                return this.cheapestAirfareWithConnections;
            }
            set
            {
                this.cheapestAirfareWithConnections = value;
            }
        }

        public decimal CheapestAirfareWithConnNonPartner
        {
            get
            {
                return this.cheapestAirfareWithConnNonPartner;
            }
            set
            {
                this.cheapestAirfareWithConnNonPartner = value;
            }
        }

        public decimal CheapestAirfareWithConnPartner
        {
            get
            {
                return this.cheapestAirfareWithConnPartner;
            }
            set
            {
                this.cheapestAirfareWithConnPartner = value;
            }
        }

        public decimal CheapestAltDate
        {
            get
            {
                return this.cheapestAltDate;
            }
            set
            {
                this.cheapestAltDate = value;
            }
        }

        public decimal CheapestNearbyAirport
        {
            get
            {
                return this.cheapestNearbyAirport;
            }
            set
            {
                this.cheapestNearbyAirport = value;
            }
        }

        public decimal CheapestRefundable
        {
            get
            {
                return this.cheapestRefundable;
            }
            set
            {
                this.cheapestRefundable = value;
            }
        }

        public decimal CheapestWithoutOffer
        {
            get
            {
                return this.cheapestWithoutOffer;
            }
            set
            {
                this.cheapestWithoutOffer = value;
            }
        }

        public decimal FirstClassAirfare
        {
            get
            {
                return this.firstClassAirfare;
            }
            set
            {
                this.firstClassAirfare = value;
            }
        }

        public string FirstClassAirfareNotShownReason
        {
            get
            {
                return this.firstClassAirfareNotShownReason;
            }
            set
            {
                this.firstClassAirfareNotShownReason = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public decimal FullYAirfare
        {
            get
            {
                return this.fullYAirfare;
            }
            set
            {
                this.fullYAirfare = value;
            }
        }

        public decimal MostExpensiveAirfare
        {
            get
            {
                return this.mostExpensiveAirfare;
            }
            set
            {
                this.mostExpensiveAirfare = value;
            }
        }

        public decimal RefundableAirfare
        {
            get
            {
                return this.refundableAirfare;
            }
            set
            {
                this.refundableAirfare = value;
            }
        }

        public decimal RefundableAverageAirfarePerPax
        {
            get
            {
                return this.refundableAverageAirfarePerPax;
            }
            set
            {
                this.refundableAverageAirfarePerPax = value;
            }
        }
    }
}
