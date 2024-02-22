using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.GooglePay
{
    [Serializable]
    public class MOBGooglePayFlightResponse : MOBResponse
    {
        private string save2GoogleUrl;
        private string classResourceId;
        private string objectResourceId;

        public string Save2GoogleUrl
        {
            get { return save2GoogleUrl; }
            set { save2GoogleUrl = value; }
        }
        public string ClassResourceId
        {
            get { return classResourceId; }
            set { classResourceId = value; }
        }
        public string ObjectResourceId
        {
            get { return objectResourceId; }
            set { objectResourceId = value; }
        }
    }
}
