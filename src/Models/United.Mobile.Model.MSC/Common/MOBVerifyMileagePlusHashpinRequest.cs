using System;
using System.Collections.Generic;
using System.Text;

namespace United.Definition.Common
{
    [Serializable]
    public class MOBVerifyMileagePlusHashpinRequest
    {
        public string TransactionId { get; set; }
        public int ApplicationId { get; set; }
        public string AppVersion { get; set; }
        public string HashValue { get; set; }
        public string MpNumber { get; set; }
        public string ServiceName { get; set; }
        public string SessionID { get; set; }
        public string DeviceId { get; set; }
    }
}
