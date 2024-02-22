
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBUITemplateBlock
    {
        //[JsonProperty("uiTemplateBlockType")]
        //public string UITemplateBlockType { get; set; }
        private string uiTemplateBlockType;
        public string UITemplateBlockType
        {
            get
            {
                return this.uiTemplateBlockType;
            }
            set
            {
                this.uiTemplateBlockType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        //[JsonProperty("content")]
        //public List<MOBUITemplateBlockContent> Content { get; set; }
        private List<MOBUITemplateBlockContent> content;
        public List<MOBUITemplateBlockContent> Content
        {
            get
            {
                return this.content;
            }
            set
            {
                this.content = value;
            }
        }
    }
}
