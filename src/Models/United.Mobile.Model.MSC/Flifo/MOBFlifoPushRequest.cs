using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Flifo
{
    [Serializable]
    public class MOBFlifoPushRequest: MOBRequest
    {
        private string pushToken;

        public string PushToken
        {
            get { return pushToken; }
            set { pushToken = value; }
        }

    }
}
