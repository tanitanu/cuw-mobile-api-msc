using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Flifo
{
    [Serializable]
    public class MOBFlifoPushResponse : MOBResponse
    {
        string message;

        public string Message
        {
            get { return message; }
            set { message = value; }
        }
    }
}
