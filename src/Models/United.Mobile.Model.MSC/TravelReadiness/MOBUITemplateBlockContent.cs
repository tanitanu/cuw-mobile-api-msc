
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBUITemplateBlockContent
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

        //[JsonProperty("contentType")]
        //public string ContentType { get; set; }
        private string contentType;
        public string ContentType
        {
            get
            {
                return this.contentType;
            }
            set
            {
                this.contentType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
