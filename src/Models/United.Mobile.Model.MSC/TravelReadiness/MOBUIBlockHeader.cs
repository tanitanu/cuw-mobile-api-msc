
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBUIBlockHeader
    {
        //[JsonProperty("header")]
        //public string Header { get; set; }
        private string header;
        public string Header
        {
            get
            {
                return this.header;
            }
            set
            {
                this.header = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        //[JsonProperty("body1")]
        //public string Body1 { get; set; }
        private string body1;
        public string Body1
        {
            get
            {
                return this.body1;
            }
            set
            {
                this.body1 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        //[JsonProperty("body2")]
        //public string Body2 { get; set; }
        private string body2;
        public string Body2
        {
            get
            {
                return this.body2;
            }
            set
            {
                this.body2 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        //[JsonProperty("body3")]
        //public string Body3 { get; set; }
        private string body3;
        public string Body3
        {
            get
            {
                return this.body3;
            }
            set
            {
                this.body3 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        //[JsonProperty("body4")]
        //public string Body4 { get; set; }
        private string body4;
        public string Body4
        {
            get
            {
                return this.body4;
            }
            set
            {
                this.body4 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        //[JsonProperty("image")]
        //public string Image { get; set; }
        private string image;
        public string Image
        {
            get
            {
                return this.image;
            }
            set
            {
                this.image = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        //[JsonProperty("color")]
        //public string Color { get; set; }
        private string color;
        public string Color
        {
            get
            {
                return this.color;
            }
            set
            {
                this.color = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
