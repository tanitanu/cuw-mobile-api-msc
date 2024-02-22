using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBFlightAvailabilitySegment : MOBSegment
    {
        private MOBAirline codeShareCarrier;
        private string codeShareFlightNumber = string.Empty;
        private MOBAirline operationoperatingCarrier;
        private MOBAircraft aircraft;
        private string serviceClass;
        private string serviceClassDescription;
        private string meal;
        private string flightTime;
        private string groundTime;
        private string totalTravelTime;
        private string actualMileage;
        private string onePassMileage;
        private string emp;
        private string totalOnePassMileage;
        private string fareBasisCode;
        private MOBShopOnTimePerformance onTimeInfo;
        //private List<MOBSegmentMessage> messages;
        private string endorsement;
        private List<MOBMessage> operatedByMessages;
        private List<MOBMessage> stopMessages;
        private List<MOBTripSegment> stopInfo;
        private bool fpwsAir;
        private int ti;
        private int si;
        private int li;
        private bool isCrossFleet;
        private string crossFleetCOFlightNumber;
        private int flightAvailabilitysegmentIndex;
        private bool cogStop;
        private bool matchServiceClassRequested;
        private List<MOBMessage> otherMessages;
        private string operatedByMessage;
        
        public MOBAirline OperationoperatingCarrier
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

        public MOBAirline CodeShareCarrier
        {
            get
            {
                return this.codeShareCarrier;
            }
            set
            {
                this.codeShareCarrier = value;
            }
        }

        public string CodeShareFlightNumber
        {
            get
            {
                return this.codeShareFlightNumber;
            }
            set
            {
                this.codeShareFlightNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBAircraft Aircraft
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

        public string ServiceClass
        {
            get
            {
                return this.serviceClass;
            }
            set
            {
                this.serviceClass = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string ServiceClassDescription
        {
            get
            {
                return this.serviceClassDescription;
            }
            set
            {
                this.serviceClassDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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
                this.flightTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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
                this.groundTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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
                this.totalTravelTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        public string OnePassMileage
        {
            get
            {
                return this.onePassMileage;
            }
            set
            {
                this.onePassMileage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        public string TotalOnePassMileage
        {
            get
            {
                return this.totalOnePassMileage;
            }
            set
            {
                this.totalOnePassMileage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBShopOnTimePerformance OnTimeInfo
        {
            get
            {
                return this.onTimeInfo;
            }
            set
            {
                this.onTimeInfo = value;
            }
        }

        public string FareBasisCode
        {
            get { return this.fareBasisCode; }
            set { this.fareBasisCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper(); }
        }

        //public List<SegmentMessage> Messages
        //{
        //    get { return this.messages; }
        //    set { this.messages = value; }
        //}

        public string Endorsement
        {
            get { return this.endorsement; }
            set { this.endorsement = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        public List<MOBMessage> OperatedByMessages
        {
            get { return this.operatedByMessages; }
            set { this.operatedByMessages = value; }
        }

        public List<MOBMessage> StopMessages
        {
            get { return this.stopMessages; }
            set { this.stopMessages = value; }
        }

        public List<MOBTripSegment> StopInfo
        {
            get { return this.stopInfo; }
            set { this.stopInfo = value; }
        }

        public bool FPWSAir
        {
            get { return this.fpwsAir; }
            set { this.fpwsAir = value; }
        }

        public int Ti
        {
            get { return this.ti; }
            set { this.ti = value; }
        }

        public int Si
        {
            get { return this.si; }
            set { this.si = value; }
        }

        public int Li
        {
            get { return this.li; }
            set { this.li = value; }
        }

        public bool IsCrossFleet
        {
            get { return this.isCrossFleet; }
            set { this.isCrossFleet = value; }
        }

        public string CrossFleetCOFlightNumber
        {
            get
            {
                return this.crossFleetCOFlightNumber;
            }
            set
            {
                this.crossFleetCOFlightNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public int FlightAvailabilitySegmentIndex
        {
            get
            {
                return this.flightAvailabilitysegmentIndex;
            }
            set
            {
                this.flightAvailabilitysegmentIndex = value;
            }
        }

        public bool COGStop
        {
            get { return this.cogStop; }
            set { this.cogStop = value; }
        }

        public bool MatchServiceClassRequested
        {
            get { return this.matchServiceClassRequested; }
            set { this.matchServiceClassRequested = value; }
        }

        public List<MOBMessage> OtherMessages
        {
            get { return this.otherMessages; }
            set { this.otherMessages = value; }
        }

        public string OperatedByMessage
        {
            get
            {
                return this.operatedByMessage;
            }
            set
            {
                this.operatedByMessage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
