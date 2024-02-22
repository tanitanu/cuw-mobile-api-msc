using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Cloud
{
    public class SaveSessionOnCloudRequest
    {


        public string TransactionId { get; set; }
        public Expirationoptions ExpirationOptions { get; set; }
        public string ObjectName { get; set; }
        public List<string> ValidationParams { get; set; }
        public string SessionId { get; set; }
        public dynamic Data { get; set; }

    }
    public class Expirationoptions
    {
        public DateTime AbsoluteExpiration { get; set; }
        public string SlidingExpiration { get; set; }
    }

    public class SaveSessionOnCloudResponse
    {
        public bool succeed { get; set; }
        public string sessionId { get; set; }
        public dynamic data { get; set; }
        public string dateTimeUtc { get; set; }
        public string machineName { get; set; }
        public int duration { get; set; }
    }
}
