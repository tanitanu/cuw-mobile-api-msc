using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable]
    public class MOBLocationEventRequest : MOBRequest
    {
        private List<MOBLocationEventResult> results;

        public List<MOBLocationEventResult> Results
        {
            get
            {
                return this.results;
            }
            set
            {
                this.results = value;
            }
        }
    }
}
