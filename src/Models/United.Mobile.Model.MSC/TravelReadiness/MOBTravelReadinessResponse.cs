
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBTravelReadinessResponse : MOBResponse
    {
        //[JsonProperty("viewController")]
        //public string ViewController { get; set; }
        private string viewController;
        public string Key
        {
            get
            {
                return this.viewController;
            }
            set
            {
                this.viewController = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        //[JsonProperty("viewMinimizedLabel")]
        //public string ViewMinimizedLabel { get; set; }
        private string viewMinimizedLabel;
        public string ViewMinimizedLabel
        {
            get
            {
                return this.viewMinimizedLabel;
            }
            set
            {
                this.viewMinimizedLabel = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        //[JsonProperty("newRelicPNR")]
        //public string NewRelicPNR { get; set; }
        private string nrPNR;
        public string NRPNR
        {
            get
            {
                return this.nrPNR;
            }
            set
            {
                this.nrPNR = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        //[JsonProperty("viewTimeToLive")]
        //public string ViewTimeToLive { get; set; }
        private string viewTimeToLive;
        public string ViewTimeToLive
        {
            get
            {
                return this.viewTimeToLive;
            }
            set
            {
                this.viewTimeToLive = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        //[JsonProperty("locationTemplate")]
        //public List<MOBUITemplateBlock> LocationTemplate { get; set; }
        private List<MOBUITemplateBlock> locationTemplate;
        public List<MOBUITemplateBlock> LocationTemplate
        {
            get
            {
                return this.locationTemplate;
            }
            set
            {
                this.locationTemplate = value;
            }
        }

        //[JsonProperty("viewContent")]
        //public MOBTravelReadinessViewContent ViewContent { get; set; }
        private MOBTravelReadinessViewContent viewContent;
        public MOBTravelReadinessViewContent ViewContent
        {
            get
            {
                return this.viewContent;
            }
            set
            {
                this.viewContent = value;
            }
        }
    }
}
