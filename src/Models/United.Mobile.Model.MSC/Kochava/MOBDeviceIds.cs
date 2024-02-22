using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Kochava
{
    [Serializable()]
    public class MOBDeviceIds
    {
        private string idfa;
        private string idfv;
        private string imei;
        private string adid;
        private string android_id;

        /// <summary>
        ///IDFA
        /// </summary>
        public string IDFA
        {
            get { return idfa; }
            set { idfa = value; }
        }

        /// <summary>
        ///IDFV
        /// </summary>
        public string IDFV
        {
            get { return idfv; }
            set { idfv = value; }
        }

        /// <summary>
        ///IMEI
        /// </summary>
        public string IMEI
        {
            get { return imei; }
            set { imei = value; }
        }

        /// <summary>
        ///ADID
        /// </summary>
        public string ADID
        {
            get { return adid; }
            set { adid = value; }
        }

        /// <summary>
        ///Android Id
        /// </summary>
        public string Android_Id
        {
            get { return android_id; }
            set { android_id = value; }
        }
    }
}
