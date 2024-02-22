using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBReleaseUpdateRequest : MOBRequest
    {
        private string osVersion = string.Empty;
        private int osVersionCounter;
        private int appVersionCounter;
        private string mileagePlusID = string.Empty;
        private string hashKey = string.Empty;
        private string custID = string.Empty;
        
        public string OSVersion
        {
            get { return osVersion; }
            set { osVersion = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper(); }
        }

        public int OSVersionCounter
        {
            get { return osVersionCounter; }
            set { osVersionCounter = value; }
        }

        public int AppVersionCounter
        {
            get { return appVersionCounter; }
            set { appVersionCounter = value; }
        }

        public string MileagePlusID
        {
            get { return mileagePlusID; }
            set { mileagePlusID = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper(); }
        }

        public string HashKey
        {
            get { return hashKey; }
            set { hashKey = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper(); }
        }

        public string CustID
        {
            get { return custID; }
            set { custID = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper(); }
        }
    }
}
