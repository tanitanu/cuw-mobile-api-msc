
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBPredictableKeyDocument
    {
        //[JsonProperty("predictableKey")]
        //public string PredictableKey { get; set; }
        private string predictableKey;
        public string PredictableKey
        {
            get
            {
                return this.predictableKey;
            }
            set
            {
                this.predictableKey = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        //[JsonProperty("headerDocument")]
        //public object HeaderDocument { get; set; }
        private object headerDocument;
        public object HeaderDocument
        {
            get
            {
                return this.headerDocument;
            }
            set
            {
                this.headerDocument = value;
            }
        }

        //[JsonProperty("flifoDocument")]
        //public object FlifoDocument { get; set; }
        private object flifoDocument;
        public object FlifoDocument
        {
            get
            {
                return this.flifoDocument;
            }
            set
            {
                this.flifoDocument = value;
            }
        }

        //[JsonProperty("weatherDocument")]
        //public object WeatherDocument { get; set; }
        private object weatherDocument;
        public object WeatherDocument
        {
            get
            {
                return this.weatherDocument;
            }
            set
            {
                this.weatherDocument = value;
            }
        }

        //[JsonProperty("tileDocument")]
        //public object TileDocument { get; set; }
        private object tileDocument;
        public object TileDocument
        {
            get
            {
                return this.tileDocument;
            }
            set
            {
                this.tileDocument = value;
            }
        }
    }
}
