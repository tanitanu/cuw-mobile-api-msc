using System;
using System.Collections.Generic;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MetaUserSessionSyncRequest
    {
        public string AuthTokenId { get; set; }
        public string CartId { get; set; }
    }
}
