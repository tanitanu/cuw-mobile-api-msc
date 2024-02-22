
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBUIListTile
    {
        //[JsonProperty("tileType")]
        //public string TileType { get; set; }
        private string tileType;
        public string TileType
        {
            get
            {
                return this.tileType;
            }
            set
            {
                this.tileType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        //[JsonProperty("icon")]
        //public string Icon { get; set; }
        private string icon;
        public string Icon
        {
            get
            {
                return this.icon;
            }
            set
            {
                this.icon = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        //[JsonProperty("colorRed")]
        //public int ColorRed { get; set; }
        private int colorRed;
        public int ColorRed
        {
            get
            {
                return this.colorRed;
            }
            set
            {
                this.colorRed = value;
            }
        }

        //[JsonProperty("colorGreen")]
        //public int ColorGreen { get; set; }
        private int colorGreen;
        public int ColorGreen
        {
            get
            {
                return this.colorGreen;
            }
            set
            {
                this.colorGreen = value;
            }
        }

        //[JsonProperty("colorBlue")]
        //public int ColorBlue { get; set; }
        private int colorBlue;
        public int ColorBlue
        {
            get
            {
                return this.colorBlue;
            }
            set
            {
                this.colorBlue = value;
            }
        }

        //[JsonProperty("view")]
        //public string View { get; set; }
        private string view;
        public string View
        {
            get
            {
                return this.view;
            }
            set
            {
                this.view = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        //[JsonProperty("url")]
        //public string Url { get; set; }
        private string url;
        public string Url
        {
            get
            {
                return this.url;
            }
            set
            {
                this.url = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        //[JsonProperty("url")]
        //public Dictionary<string, string> Url { get; set; }
        private Dictionary<string, string> labels;
        public Dictionary<string, string> Labels
        {
            get
            {
                return this.labels;
            }
            set
            {
                this.labels = value;
            }
        }
    }
}
