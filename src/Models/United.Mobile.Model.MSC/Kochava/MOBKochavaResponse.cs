using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Kochava
{
    [Serializable]
   public class MOBKochavaResponse : MOBResponse
   {
        private string status;
        private string guid = string.Empty;
        private int deviceID;
        private bool catalogValue;

        public MOBKochavaResponse()
            : base()
        {
        }

        public string Status
        {
            get { return status; }
            set { status = value; }
        }

        public string GUID
        {
            get
            {
                return this.guid;
            }
            set
            {
                this.guid = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public int DeviceID
        {
            get
            {
                return this.deviceID;
            }
            set
            {
                this.deviceID = value;
            }
        }

        public bool CatalogValue
        {
            get
            {
                return this.catalogValue;
            }
            set
            {
                this.catalogValue = value;
            }
        }
    }
}
