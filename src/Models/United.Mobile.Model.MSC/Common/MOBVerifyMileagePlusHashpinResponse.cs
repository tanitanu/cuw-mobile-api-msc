using System;
using System.Collections.Generic;
using System.Text;

namespace United.Definition.Common
{
    [Serializable]
    public class MOBVerifyMileagePlusHashpinResponse
    {

        
        public Mpdetails mpDetails { get; set; }
        public string message { get; set; }
        public object messageCode { get; set; }
        public string transactionId { get; set; }
        public string languageCode { get; set; }
        public string machineName { get; set; }
        public int callDuration { get; set; }
        public object exception { get; set; }

    }

    public class Mpdetails
    {
        public string isTouchIDSignIn { get; set; }
        public string tokenExpiryInSeconds { get; set; }
        public string tokenExpireDateTime { get; set; }
        public string isTokenValid { get; set; }
        public object authenticatedToken { get; set; }
        public string dataPowerAccessToken { get; set; }
        public string deviceID { get; set; }
        public string appVersion { get; set; }
        public string applicationID { get; set; }
        public string pinCode { get; set; }
        public string hashPincode { get; set; }
        public string mpUserName { get; set; }
        public string mileagePlusNumber { get; set; }
        public string isTokenAnonymous { get; set; }
        public string customerID { get; set; }
        public string updateDateTime { get; set; }
        public int callDuration { get; set; }
    }
}
