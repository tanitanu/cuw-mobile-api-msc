using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable]
    public class CouchEquipment
    {
        //[JsonProperty("id")]
        //public string Id { get; set; }
        private string id;
        public string Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        //[JsonProperty("noseNumber")]
        //public string NoseNumber { get; set; }
        private string noseNumber;
        public string NoseNumber
        {
            get
            {
                return this.noseNumber;
            }
            set
            {
                this.noseNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        //[JsonProperty("tailNumber")]
        //public string TailNumber { get; set; }
        private string tailNumber;
        public string TailNumber
        {
            get
            {
                return this.tailNumber;
            }
            set
            {
                this.tailNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        //[JsonProperty("aircaft")]
        //public CouchAircraft Aircraft { get; set; }
        private CouchAircraft aircaft;
        public CouchAircraft Aircraft
        {
            get
            {
                return this.aircaft;
            }
            set
            {
                this.aircaft = value;
            }
        }
    }
}