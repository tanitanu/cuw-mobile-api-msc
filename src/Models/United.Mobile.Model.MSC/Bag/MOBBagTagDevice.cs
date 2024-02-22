using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Bag
{
    [Serializable]
    public class MOBBagTagDevice
    {
        private string mileagePlusNumber = string.Empty;
        private int vendorId;
        private string vendorName = string.Empty;
        private string deviceSerialNumbner = string.Empty;

        public string MileagePlusNumber
        {
            get
            {
                return mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public int VendorId
        {
            get
            {
                return vendorId;
            }
            set
            {
                this.vendorId = value;
            }
        }

        public string VendorName
        {
            get
            {
                return vendorName;
            }
            set
            {
                this.vendorName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string DeviceSerialNumbner
        {
            get
            {
                return deviceSerialNumbner;
            }
            set
            {
                this.deviceSerialNumbner = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }
    }
}
