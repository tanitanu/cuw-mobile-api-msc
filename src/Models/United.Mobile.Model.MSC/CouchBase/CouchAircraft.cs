using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable]
    public class CouchAircraft
    {
        //[JsonProperty("code")]
        //public string Code { get; set; }
        private string code;
        public string Code
        {
            get
            {
                return this.code;
            }
            set
            {
                this.code = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        //[JsonProperty("shortName")]
        //public string ShortName { get; set; }
        private string shortName;
        public string ShortName
        {
            get
            {
                return this.shortName;
            }
            set
            {
                this.shortName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        //[JsonProperty("longName")]
        //public string LongName { get; set; }
        private string longName;
        public string LongName
        {
            get
            {
                return this.longName;
            }
            set
            {
                this.longName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}