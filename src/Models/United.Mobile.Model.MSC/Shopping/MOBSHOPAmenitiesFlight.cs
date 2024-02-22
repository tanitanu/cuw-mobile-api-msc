using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBSHOPAmenitiesFlight
    {
        private string tripId = string.Empty;
        private string flightId = string.Empty;
        private string flightNumber = string.Empty;

        private bool hasWifi = false;
        private bool hasInSeatPower = false;
        private bool hasDirecTV = false;
        private bool hasAVOnDemand = false;
        private bool hasBeverageService = false;
        private bool hasEconomyLieFlatSeating = false;
        private bool hasEconomyMeal = false;
        private bool hasFirstClassMeal = false;
        private bool hasFirstClassLieFlatSeating = false;

        public string TripId
        {
            get
            {
                return this.tripId;
            }
            set
            {
                this.tripId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string FlightId
        {
            get
            {
                return this.flightId;
            }
            set
            {
                this.flightId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
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
        public bool HasWifi
        {
            get
            {
                return this.hasWifi;
            }
            set
            {
                this.hasWifi = value;
            }
        }
        public bool HasInSeatPower
        {
            get
            {
                return this.hasInSeatPower;
            }
            set
            {
                this.hasInSeatPower = value;
            }
        }
        public bool HasDirecTV
        {
            get
            {
                return this.hasDirecTV;
            }
            set
            {
                this.hasDirecTV = value;
            }
        }
        public bool HasAVOnDemand
        {
            get
            {
                return this.hasAVOnDemand;
            }
            set
            {
                this.hasAVOnDemand = value;
            }
        }
        public bool HasBeverageService
        {
            get
            {
                return this.hasBeverageService;
            }
            set
            {
                this.hasBeverageService = value;
            }
        }
        public bool HasEconomyLieFlatSeating
        {
            get
            {
                return this.hasEconomyLieFlatSeating;
            }
            set
            {
                this.hasEconomyLieFlatSeating = value;
            }
        }
        public bool HasEconomyMeal
        {
            get
            {
                return this.hasEconomyMeal;
            }
            set
            {
                this.hasEconomyMeal = value;
            }
        }
        public bool HasFirstClassMeal
        {
            get
            {
                return this.hasFirstClassMeal;
            }
            set
            {
                this.hasFirstClassMeal = value;
            }
        }
        public bool HasFirstClassLieFlatSeating
        {
            get
            {
                return this.hasFirstClassLieFlatSeating;
            }
            set
            {
                this.hasFirstClassLieFlatSeating = value;
            }
        }

    }
}
