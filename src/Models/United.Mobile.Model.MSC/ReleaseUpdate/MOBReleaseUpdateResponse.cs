using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBReleaseUpdateResponse : MOBResponse
    {
        private string displayMessage = string.Empty;
        private string ctaRemindMe = string.Empty;
        private string ctaUpdateNow = string.Empty;
        private string ctaDismiss = string.Empty;
        private int remindMeInterval_Hours;
        private string remindMeInterval_DateTime;
        private int logIt;

        public string DisplayMessage
        {
            get { return displayMessage; }
            set { displayMessage = value; }
        }

        public string CTARemindMe
        {
            get { return ctaRemindMe; }
            set { ctaRemindMe = value; }
        }

        public string CTAUpdateNow
        {
            get { return ctaUpdateNow; }
            set { ctaUpdateNow = value; }
        }

        public string CTADismiss
        {
            get { return ctaDismiss; }
            set { ctaDismiss = value; }
        }

        public int RemindMeInterval_Hours
        {
            get { return remindMeInterval_Hours; }
            set { remindMeInterval_Hours = value; }
        }

        public string RemindMeInterval_DateTime
        {
            get { return remindMeInterval_DateTime; }
            set { remindMeInterval_DateTime = value; }
        }
        public int LogIt
        {
            get { return logIt; }
            set { logIt = value; }
        }
    }
}
