using System;
using System.Runtime.Serialization;
using United.Mobile.Model.Common;
namespace United.Definition.Kochava
{
    [Serializable()]
    public class MOBKochavaRequest :MOBRequest
    {
        // string name, string model, string localizedModel, string systemName, string systemVersion, string applicationVersion
        private string sessionId = string.Empty;
        //private string name;
        private string model;
        private string localizedModel;
        private string systemName;
        private string systemVersion;
        private string applicationVersion;        
        private string applicationId;
        private string deviceId;
        private MOBData data;

        public string ApplicationVersion
        {
            get { return applicationVersion; }
            set { applicationVersion = value; }
        }


        public string SystemVersion
        {
            get { return systemVersion; }
            set { systemVersion = value; }
        }

        public string SystemName
        {
            get { return systemName; }
            set { systemName = value; }
        }

        public string LocalizedModel
        {
            get { return localizedModel; }
            set { localizedModel = value; }
        }

        public string Model
        {
            get { return model; }
            set { model = value; }
        }

        //public string Name
        //{
        //    get { return name; }
        //    set { name = value; }
        //}       

        /// <summary>
        /// session Id
        /// </summary>
        public string SessionId
        {
            get { return sessionId; }
            set { sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        /// <summary>
        /// DeviceId
        /// </summary>
        public string DeviceId
        {
            get { return deviceId; }
            set { deviceId = value; }
        }

        public string ApplicationId
        {
            get { return applicationId; }
            set { applicationId = value; }
        }
        /// <summary>
        ///Data
        /// </summary>        
        public MOBData Data
        {
            get { return data; }
            set { data = value; }
        }

    }

}
