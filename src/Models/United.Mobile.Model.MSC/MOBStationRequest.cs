using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBStationRequest : MOBRequest
    {
        private string availableFlag;

        public string AvailableFlag
        {
             get
             {
                 return this.availableFlag;
             }
             set
             {
                 this.availableFlag = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
             }
        }
    }
}
