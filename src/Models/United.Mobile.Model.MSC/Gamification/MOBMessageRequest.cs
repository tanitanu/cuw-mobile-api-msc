using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Gamification
{
    public class MOBMessageRequest
    {
        public string PushToken { get; set; }

        public string MPNumber { get; set; }

        public string MessageTitle { get; set; }

        public string Message { get; set; }

        public string MessageType { get; set; }

        public string MessageSubType { get; set; }

        public bool PushNotificationSent { get; set; }

        public string Icon { get; set; }

        public string User { get; set; }   

    }
}
