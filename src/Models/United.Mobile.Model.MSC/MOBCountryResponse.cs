using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBCountryResponse : MOBResponse
    {
        public MOBCountryResponse()
            : base()
        {
        }

        private List<MOBCountryItem> countries;
        private string version = string.Empty;

        public List<MOBCountryItem> Countries
        {
            get { return this.countries; }
            set
            {
                this.countries = value;
            }
        }

        public string Version
        {
            get
            {
                return this.version;
            }
            set
            {
                this.version = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
    }
}
