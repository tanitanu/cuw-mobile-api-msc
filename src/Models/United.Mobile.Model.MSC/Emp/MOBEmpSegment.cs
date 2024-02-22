using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Emp.Common;

namespace United.Definition.Emp
{
    [Serializable]
    public class MOBEmpSegment
    {
        private string flightNumber;
        private MOBEmpAirport origin;
        private string departureTime;
        private MOBEmpAirport destination;
        private string arrivalTime;
        private MOBEmpTravelTime travelTime;
        private MOBEmpSegmentPBT mobEmpSegmentPBT;
        private string firstClassBucket;
        private string departureDate;
        private string arrivalDate;
        private string marketingCarrier;
        private string operatingCarrier;
        private string carrierMessage;
        private string upgradableStandby;
        private MOBTypeOption meal;
        private string equipment;
        private bool isRevenueStandBy;
        private List<string> notes;
        private bool isBusinessFirstEligible;
        private bool isBusinessCoachEligible;
        private MOBEmpPSCost psCost;

        public string FlightNumber
        {
            get { return this.flightNumber; }
            set { this.flightNumber = value; }
        }
        public MOBEmpAirport Origin
        {
            get { return this.origin; }
            set { this.origin = value; }
        }
        public string DepartureTime
        {
            get { return this.departureTime; }
            set { this.departureTime = value; }
        }
        public MOBEmpAirport Destination
        {
            get { return this.destination; }
            set { this.destination = value; }
        }
        public string ArrivalTime
        {
            get { return this.arrivalTime; }
            set { this.arrivalTime = value; }
        }
        public MOBEmpTravelTime TravelTime
        {
            get { return this.travelTime; }
            set { this.travelTime = value; }
        }
        public string FirstClassBucket
        {
            get { return this.firstClassBucket; }
            set { this.firstClassBucket = value; }
        }
        public string DepartureDate
        {
            get { return this.departureDate; }
            set { this.departureDate = value; }
        }
        public string ArrivalDate
        {
            get { return this.arrivalDate; }
            set { this.arrivalDate = value; }
        }

        public string MarketingCarrier
        {
            get { return this.marketingCarrier; }
            set { this.marketingCarrier = value; }
        }

        public string OperatingCarrier
        {
            get { return this.operatingCarrier; }
            set { this.operatingCarrier = value; }
        }

        public string CarrierMessage
        {
            get { return this.carrierMessage; }
            set { this.carrierMessage = value; }
        }

        public string UpgradableStandBy
        {
            get { return this.upgradableStandby; }
            set { this.upgradableStandby = string.IsNullOrEmpty(value) ? "n/a" : value.Trim(); }
        }

        public MOBTypeOption Meal
        {
            get { return this.meal; }
            set { this.meal = value; }
        }

        public string Equipment
        {
            get { return this.equipment; }
            set { this.equipment = value; }
        }

        public bool IsRevenueStandBy
        {
            get { return this.isRevenueStandBy; }
            set { this.isRevenueStandBy = value; }
        }

        public List<string> Notes
        {
            get { return this.notes; }
            set { this.notes = value; }
        }

        public bool BookBusinessFirst
        {
            get { return isBusinessFirstEligible; }
            set { isBusinessFirstEligible = value; }
        }

        public bool BookBusinessCoach
        {
            get { return isBusinessCoachEligible; }
            set { isBusinessCoachEligible = value; }
        }

        public MOBEmpPSCost PSCost
        {
            get { return psCost; }
            set { psCost = value; }

        }

        public MOBEmpSegmentPBT MobEmpSegmentPBT
        {
            get {return mobEmpSegmentPBT;}
            set { mobEmpSegmentPBT = value; }
        }
    }
}
