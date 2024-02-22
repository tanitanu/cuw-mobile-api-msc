using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
      [Serializable]
    public class MOBDOTBaggageInfoRequest : MOBRequest
    {
        private List<MOBDOTBaggageFlightSegment> flightSegments;
        private List<MOBDOTBaggageTravelerInfo> traverlerINfo;
        private string recordLocator;
        private string lastName;

        public List<MOBDOTBaggageFlightSegment> FlightSegments
        {
            get { return this.flightSegments; }
            set { this.flightSegments = value; }
        }

        public List<MOBDOTBaggageTravelerInfo> TraverlerINfo
        {
            get { return this.traverlerINfo; }
            set { this.traverlerINfo = value; }
        }

        public string RecordLocator
        {
            get { return this.recordLocator; }
            set { this.recordLocator = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper(); }
        }

        public string LastName
        {
            get { return this.lastName; }
            set { this.lastName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
    }
}

