using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable]
    public class CouchAirport
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
        //[JsonProperty("name")]
        //public string Name { get; set; }
        private string name;
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        //[JsonProperty("city")]
        //public string City { get; set; }
        private string city;
        public string City
        {
            get
            {
                return this.city;
            }
            set
            {
                this.city = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}