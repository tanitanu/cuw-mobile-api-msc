using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Location
{
    [Serializable]
    public class MOBSITAMetaData
    {
        private string descriptiveText = string.Empty;

        public string DescriptiveText
        {
            get
            {
                return this.descriptiveText;
            }
            set
            {
                this.descriptiveText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
