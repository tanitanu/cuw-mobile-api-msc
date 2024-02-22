using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Emp.MP2015;

namespace United.Definition.Emp.Common
{
    [Serializable()]
    public class MOBEmpPNRSegment : MOBEmpSeg
    {
        private MOBEmpAirline operationoperatingCarrier;
        private MOBEmpAircraft aircraft;
        private string meal = string.Empty;
        private string flightTime = string.Empty;
        private string groundTime = string.Empty;
        private string totalTravelTime = string.Empty;
        private string actualMileage = string.Empty;
        private string mileagePlusMileage = string.Empty;
        private string emp = string.Empty;
        private string totalMileagePlusMileage = string.Empty;

        private string classOfService = string.Empty;
        private string classOfServiceDescription = string.Empty;
        private List<MOBEmpPNRSeat> seats = new List<MOBEmpPNRSeat>();
        private string numberOfStops = string.Empty;
        private List<MOBEmpPNRSegment> stops = new List<MOBEmpPNRSegment>();

        private string scheduledDepartureDateTimeGMT = string.Empty;
        private string scheduledArrivalDateTimeGMT = string.Empty;

        private MOBEmpAirline codeshareCarrier = new MOBEmpAirline();
        private string codeshareFlightNumber = string.Empty;

        private MOBEmpSegmentResponse upgradeVisibility;
        private int lowestEliteLevel;
        private bool upgradeable;
        private string upgradeableMessageCode = string.Empty;
        private string upgradeableMessage = string.Empty;
        private string upgradeableRemark = string.Empty;
        private string complimentaryInstantUpgradeMessage = string.Empty;

        private bool remove;
        private bool waitlisted;
        private string actionCode = string.Empty;

        private bool upgradeEligible;

        private string waitedCOSDesc = string.Empty;

        private List<MOBEmpLmxProduct> lmxProducts;

        private string cabinType = string.Empty;

        private bool nonPartnerFlight = false;

        private List<MOBEmpBundle> bundles;

        public MOBEmpPNRSegment()
            : base()
        {
        }

        public List<MOBEmpBundle> Bundles
        {
            get
            {
                return this.bundles;
            }
            set
            {
                this.bundles = value;
            }
        }

        public MOBEmpAirline OperationoperatingCarrier
        {
            get
            {
                return this.operationoperatingCarrier;
            }
            set
            {
                this.operationoperatingCarrier = value;
            }
        }

        public MOBEmpAircraft Aircraft
        {
            get
            {
                return this.aircraft;
            }
            set
            {
                this.aircraft = value;
            }
        }

        public string Meal
        {
            get
            {
                return this.meal;
            }
            set
            {
                this.meal = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FlightTime
        {
            get
            {
                return this.flightTime;
            }
            set
            {
                this.flightTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string GroundTime
        {
            get
            {
                return this.groundTime;
            }
            set
            {
                this.groundTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string TotalTravelTime
        {
            get
            {
                return this.totalTravelTime;
            }
            set
            {
                this.totalTravelTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string ActualMileage
        {
            get
            {
                return this.actualMileage;
            }
            set
            {
                this.actualMileage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string MileagePlusMileage
        {
            get
            {
                return this.mileagePlusMileage;
            }
            set
            {
                this.mileagePlusMileage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string EMP
        {
            get
            {
                return this.emp;
            }
            set
            {
                this.emp = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string TotalMileagePlusMileage
        {
            get
            {
                return this.totalMileagePlusMileage;
            }
            set
            {
                this.totalMileagePlusMileage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ClassOfService
        {
            get
            {
                return this.classOfService;
            }
            set
            {
                this.classOfService = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string ClassOfServiceDescription
        {
            get
            {
                return this.classOfServiceDescription;
            }
            set
            {
                this.classOfServiceDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBEmpPNRSeat> Seats
        {
            get
            {
                return this.seats;
            }
            set
            {
                this.seats = value;
            }
        }

        public string NumberOfStops
        {
            get
            {
                return this.numberOfStops;
            }
            set
            {
                this.numberOfStops = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBEmpPNRSegment> Stops
        {
            get
            {
                return this.stops;
            }
            set
            {
                this.stops = value;
            }
        }

        public string ScheduledDepartureDateTimeGMT
        {
            get
            {
                return this.scheduledDepartureDateTimeGMT;
            }
            set
            {
                this.scheduledDepartureDateTimeGMT = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ScheduledArrivalDateTimeGMT
        {
            get
            {
                return this.scheduledArrivalDateTimeGMT;
            }
            set
            {
                this.scheduledArrivalDateTimeGMT = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBEmpAirline CodeshareCarrier
        {
            get
            {
                return this.codeshareCarrier;
            }
            set
            {
                this.codeshareCarrier = value;
            }
        }

        public string CodeshareFlightNumber
        {
            get
            {
                return this.codeshareFlightNumber;
            }
            set
            {
                this.codeshareFlightNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBEmpSegmentResponse UpgradeVisibility
        {
            get
            {
                return this.upgradeVisibility;
            }
            set
            {
                this.upgradeVisibility = value;
            }
        }

        public bool Upgradeable
        {
            get
            {
                return this.upgradeable;
            }
            set
            {
                this.upgradeable = value;
            }
        }

        public string UpgradeableMessageCode
        {
            get
            {
                return this.upgradeableMessageCode;
            }
            set
            {
                this.upgradeableMessageCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string UpgradeableMessage
        {
            get
            {
                return this.upgradeableMessage;
            }
            set
            {
                this.upgradeableMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ComplimentaryInstantUpgradeMessage
        {
            get
            {
                return this.complimentaryInstantUpgradeMessage;
            }
            set
            {
                this.complimentaryInstantUpgradeMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool Remove
        {
            get
            {
                return this.remove;
            }
            set
            {
                this.remove = value;
            }
        }

        public bool Waitlisted
        {
            get
            {
                return this.waitlisted;
            }
            set
            {
                this.waitlisted = value;
            }
        }

        public int LowestEliteLevel
        {
            get
            {
                return this.lowestEliteLevel;
            }
            set
            {
                this.lowestEliteLevel = value;
            }
        }

        public string UpgradeableRemark
        {
            get
            {
                return this.upgradeableRemark;
            }
            set
            {
                this.upgradeableRemark = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ActionCode
        {
            get
            {
                return this.actionCode;
            }
            set
            {
                this.actionCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public bool UpgradeEligible
        {
            get
            {
                return this.upgradeEligible;
            }
            set
            {
                this.upgradeEligible = value;
            }
        }

        public string WaitedCOSDesc
        {
            get
            {
                return this.waitedCOSDesc;
            }
            set
            {
                this.waitedCOSDesc = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBEmpLmxProduct> LmxProducts
        {
            get
            {
                return this.lmxProducts;
            }
            set
            {
                this.lmxProducts = value;
            }
        }

        public string CabinType
        {
            get
            {
                return this.cabinType;
            }
            set
            {
                this.cabinType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool NonPartnerFlight
        {
            get
            {
                return this.nonPartnerFlight;
            }
            set
            {
                this.nonPartnerFlight = value;
            }
        }
    }
}
