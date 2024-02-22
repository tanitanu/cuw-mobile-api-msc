using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Kochava
{
    [Serializable()]
    public class MOBKochavaData
    {
        private string origination_ip;
        private string device_ver;
        private MOBDeviceIds device_ids;

        public string Origination_Ip
        {
            get { return origination_ip; }
            set { origination_ip = value; }
        }

        /// <summary>
        ///Device Version
        /// </summary>
        public string Device_Ver
        {
            get { return device_ver; }
            set { device_ver = value; }
        }

        /// <summary>
        ///Device Ids
        /// </summary>        
        public MOBDeviceIds Device_Ids
        {
            get { return device_ids; }
            set { device_ids = value; }
        }

    }
}