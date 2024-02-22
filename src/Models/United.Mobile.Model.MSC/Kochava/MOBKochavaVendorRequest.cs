using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Kochava
{
   public class MOBKochavaVendorRequest
    {
        private string Action;
        private string Kochava_App_Id;
        private string App_ver;
        private MOBKochavaData Data;

        /// <summary>
        /// Kochava_App_Id
        /// </summary>
        public string kochava_app_id
        {
            get { return Kochava_App_Id; }
            set { Kochava_App_Id = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
        /// <summary>
        ///Data
        /// </summary>        
        public MOBKochavaData data
        {
            get { return Data; }
            set { Data = value; }
        }

        ///// <summary>
        ///// Application Version
        ///// </summary>
        public string app_ver
        {
            get { return App_ver; ; }
            set { App_ver = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        ///// <summary>
        ///// Action
        ///// </summary>
        public string action
        {
            get { return Action; }
            set { Action = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
    }
}
