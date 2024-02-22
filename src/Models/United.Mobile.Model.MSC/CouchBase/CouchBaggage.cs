using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable]
    public class CouchBaggage
    {
        //[JsonProperty("bagTerminal")]
        //public string BagTerminal { get; set; }
        private string bagTerminal;
        public string BagTerminal
        {
            get
            {
                return this.bagTerminal;
            }
            set
            {
                this.bagTerminal = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        //[JsonProperty("bagClaimUnit")]
        //public string BagClaimUnit { get; set; }
        private string bagClaimUnit;
        public string BagClaimUnit
        {
            get
            {
                return this.bagClaimUnit;
            }
            set
            {
                this.bagClaimUnit = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        //[JsonProperty("hasBagLocation")]
        //public bool HasBagLocation { get; set; }
        private bool hasBagLocation;
        public bool HasBagLocation
        {
            get
            {
                return this.hasBagLocation;
            }
            set
            {
                this.hasBagLocation = value;
            }
        }
    }
}