using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace United.Definition
{
    [Serializable()]
    [DataContract]
    public class ScheduleSegment
    {
        //User Story 160153 - Added the datacontract and datamember attributes.
        [DataMember]
        private string leg = string.Empty;
        [DataMember]
        private string stops = string.Empty;
        [DataMember]
        private string origin = string.Empty;
        [DataMember]
        private string originName = string.Empty;
        [DataMember]
        private string destination = string.Empty;
        [DataMember]
        private string destinationName = string.Empty;
        [DataMember]
        private string flightNumber = string.Empty;
        [DataMember]
        private string displayFlightNumber = string.Empty;
        [DataMember]
        private string codeShareFlightNumber = string.Empty;
        [DataMember]
        private string departureDate = string.Empty;
        [DataMember]
        private string departureTime = string.Empty;
        [DataMember]
        private string departureTimeSort = string.Empty;
        [DataMember]
        private string arrivalTime = string.Empty;
        [DataMember]
        private string arrivalOffset = string.Empty;
        [DataMember]
        private string equipment = string.Empty;
        [DataMember]
        private string mealTemp = string.Empty;
        [DataMember]
        private string classOfService = string.Empty;
        [DataMember]
        private string fcMeal = string.Empty;
        [DataMember]
        private string ecMeal = string.Empty;
        [DataMember]
        private string miles = string.Empty;
        [DataMember]
        private string travelTime = string.Empty;
        [DataMember]
        private string travelTimeSort = string.Empty;
        [DataMember]
        private string eliteQualification = string.Empty;
        [DataMember]
        private string onTimePercentage = string.Empty;
        [DataMember]
        private string svcMap = string.Empty;
        [DataMember]
        private string operatedBy = string.Empty;
        [DataMember]
        private string operatedByCode = string.Empty;
        [DataMember]
        private string marketedBy = string.Empty;
        // User Story 160153 - Added below two variables compared with new rest
        [DataMember]
        private string arrivalDateTime = string.Empty;
        [DataMember]
        private string departureDateTime = string.Empty;


        [XmlIgnore]
        public string Leg
        {
            get 
            { 
                return leg; 
            }
            set
            {
                this.leg = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        [XmlIgnore]
        public string Stops
        {
            get
            { 
                return stops; 
            }
            set
            {
                this.stops = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        
        public string Origin
        {
            get 
            { 
                return origin; 
            }
            set
            {
                this.origin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string OriginName
        {
            get
            {
                return originName;
            }
            set
            {
                this.originName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
                if (this.originName.IndexOf('-') != -1)
                {
                    int pos = this.originName.IndexOf('-');
                    this.originName = string.Format("{0})", this.originName.Substring(0, pos).Trim());
                }

            }
        }

        public string Destination
        {
            get
            { 
                return destination; 
            }
            set
            {
                this.destination = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string DestinationName
        {
            get
            {
                return destinationName;
            }
            set
            {
                this.destinationName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
                if (this.destinationName.IndexOf('-') != -1)
                {
                    int pos = this.destinationName.IndexOf('-');
                    this.destinationName = string.Format("{0})", this.destinationName.Substring(0, pos).Trim());
                }
            }
        }

        public string FlightNumber      
        {
            get
            { 
                return flightNumber; 
            }
            set
            {
                this.flightNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string DisplayFlightNumber
        {
            get
            {
                return displayFlightNumber;
            }
            set
            {
                this.displayFlightNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string CodeShareFlightNumber
        {
            get
            {
                return codeShareFlightNumber;
            }
            set
            {
                this.codeShareFlightNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }


        public string DepartureDate
        {
            get
            {
                    return departureDate;
            }
            set
            {
                    this.departureDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        //User Story 160153 - Modified properties compared with new rest
        public string DepartureTime
        {
            get
            {
                if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["EnableFlightSearchDetailsByCity"])
                    && Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["EnableFlightSearchDetailsByCity"]))
                {
                    return DepartureDateTime;
                }
                else
                {
                    return departureTime; 
                }
            }
            set
            {
                    this.departureTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        //User Story 160153 - Modified properties compared with new rest
        public string ArrivalTime
        {
            get
            {
                if (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["EnableFlightSearchDetailsByCity"])
                    && Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["EnableFlightSearchDetailsByCity"]))
                {
                    return ArrivalDateTime;
                }
                else
                {
                    return this.arrivalTime;                
                }
            }
            set
            {
                   this.arrivalTime = string.IsNullOrEmpty(value) ? null : value.Trim();
            }
        }

        [XmlIgnore]
        public string DepartureTimeSort
        {
            get 
            { 
                return departureTimeSort; 
            }
            set
            {
                this.departureTimeSort = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }


        //User Story 160153 - Added properties compared with new rest
        public string ArrivalDateTime
        {
            get { return this.arrivalDateTime; }
            set { this.arrivalDateTime = string.IsNullOrEmpty(value) ? string.Empty : Convert.ToDateTime(value).ToString(@"h:mmm tt"); }
        }

        //User Story 160153 - Added properties compared with new rest
        public string DepartureDateTime
        {
            get { return this.departureDateTime; }
            set { this.departureDateTime = string.IsNullOrEmpty(value) ? string.Empty : Convert.ToDateTime(value).ToString(@"h:mmm tt"); }
        }

        public string ArrivalOffset
        {
            get 
            { 
                return this.arrivalOffset; 
            }
            set
            {
                this.arrivalOffset = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Equipment
        {
            get 
            { 
                return this.equipment; 
            }
            set
            {
                this.equipment = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FirstClassMeal
        {
            get 
            { 
                return this.fcMeal; 
            }
            set
            {
                this.fcMeal = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string EconomyClassMeal
        {
            get
            {
                return this.ecMeal;
            }
            set
            {
                this.ecMeal = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string MealTemp
        {
            get
            {
                return this.mealTemp;
            }
            set
            {
                this.mealTemp = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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
                this.classOfService = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string EliteQualification    
        {
            get
            {
                return this.eliteQualification;
            }
            set
            {
                this.eliteQualification = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string OnTimePercentage
        {
            get
            {
                return this.onTimePercentage;
            }
            set
            {
                this.onTimePercentage = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }


        public string Miles
        {
            get 
            { 
                return this.miles; 
            }
            set
            {
                this.miles = string.IsNullOrEmpty(value) ? string.Empty : value.TrimStart('0');
            }
        }


        public string TravelTime
        {
            get 
            { 
                return this.travelTime; 
            }
            set
            {
                this.travelTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string TravelTimeSort
        {
            get
            {
                return this.travelTimeSort;
            }
            set
            {
                this.travelTimeSort = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }


        [XmlIgnore]
        public string SVCMap
        {
            get
            { 
                return this.svcMap; 
            }
            set
            {
                this.svcMap = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        
        public string OperatedBy
        {
            get
            {
                return this.operatedBy;
            }
            set
            {
                this.operatedBy = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        public string OperatedByCode
        {
            get
            {
                return this.operatedByCode;
            }
            set
            {
                this.operatedByCode = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        public string MarketedBy
        {
            get
            {
                return this.marketedBy;
            }
            set
            {
                this.marketedBy = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }
    }
}
