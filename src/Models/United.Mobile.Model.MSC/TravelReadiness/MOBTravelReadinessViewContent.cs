
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBTravelReadinessViewContent
    {
        //[JsonProperty("header")]
        //public List<MOBPredictableKeyDocument> Header { get; set; }
        private List<MOBPredictableKeyDocument> header;
        public List<MOBPredictableKeyDocument> Header
        {
            get
            {
                return this.header;
            }
            set
            {
                this.header = value;
            }
        }

        //[JsonProperty("weatherMinimized")]
        //public List<MOBPredictableKeyDocument> WeatherMinimized { get; set; }
        private List<MOBPredictableKeyDocument> weatherMinimized;
        public List<MOBPredictableKeyDocument> WeatherMinimized
        {
            get
            {
                return this.weatherMinimized;
            }
            set
            {
                this.weatherMinimized = value;
            }
        }

        //[JsonProperty("baggageMinimized")]
        //public List<MOBPredictableKeyDocument> BaggageMinimized { get; set; }
        private List<MOBPredictableKeyDocument> baggageMinimized;
        public List<MOBPredictableKeyDocument> BaggageMinimized
        {
            get
            {
                return this.baggageMinimized;
            }
            set
            {
                this.baggageMinimized = value;
            }
        }

        //[JsonProperty("flightStatusMinimized")]
        //public List<MOBPredictableKeyDocument> FlightStatusMinimized { get; set; }
        private List<MOBPredictableKeyDocument> flightStatusMinimized;
        public List<MOBPredictableKeyDocument> FlightStatusMinimized
        {
            get
            {
                return this.flightStatusMinimized;
            }
            set
            {
                this.flightStatusMinimized = value;
            }
        }

        //[JsonProperty("flightStatusMinimized")]
        //public List<MOBPredictableKeyDocument> FlightStatusMinimized { get; set; }
        private List<MOBPredictableKeyDocument> listTile;
        public List<MOBPredictableKeyDocument> ListTile
        {
            get
            {
                return this.listTile;
            }
            set
            {
                this.listTile = value;
            }
        }
    }
}
