using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBCodeTableResponse : MOBResponse
    {
        private List<MOBCountry> countries;

        public MOBCodeTableResponse()
            : base()
        {
        }

        public List<MOBCountry> Countries
        {
            get
            {
                return this.countries;
            }
            set
            {
                this.countries = value;
            }
        }
    }
}
