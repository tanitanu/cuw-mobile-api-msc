using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBTVListingRequest : MOBRequest
    {
        public MOBTVListingRequest()
            : base()
        {
        }

        private string timeZone = string.Empty;
        private string startDate = string.Empty;
        private string endDate = string.Empty;

        public string TimeZone
        {
            get
            {
                return this.timeZone;
            }
            set
            {
                this.timeZone = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string StartDate
        {
            get
            {
                return this.startDate;
            }
            set
            {
                this.startDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string EndDate
        {
            get
            {
                return this.endDate;
            }
            set
            {
                this.endDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
    }
}
