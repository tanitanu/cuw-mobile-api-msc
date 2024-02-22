using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBSHOPOrganizeResultsReqeust : MOBRequest
    {
        private string sessionId = string.Empty;
        private MOBSHOPSearchFilters searchFiltersIn;
        private int lastTripIndexRequested;

        public string SessionId
        {
            get
            {
                return this.sessionId;
            }
            set
            {
                this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public MOBSHOPSearchFilters SearchFiltersIn
        {
            get
            {
                return this.searchFiltersIn;
            }
            set
            {
                this.searchFiltersIn = value;
            }
        }
        public int LastTripIndexRequested
        {
            get
            {
                return this.lastTripIndexRequested;
            }
            set
            {
                this.lastTripIndexRequested = value;
            }
        }
    }
}
