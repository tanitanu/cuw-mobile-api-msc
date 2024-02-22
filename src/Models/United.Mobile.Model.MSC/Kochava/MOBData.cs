using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Kochava
{
    [Serializable()]
    public class MOBData
    {
        private string origination_ip;
        private string device_ver;
        private MOBDeviceIds device_Ids;

        public string Origination_IP
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
            get { return device_Ids; }
            set { device_Ids = value; }
        }

    }
}
