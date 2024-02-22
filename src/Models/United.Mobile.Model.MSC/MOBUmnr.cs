using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBUmnr
    {
        private List<MOBItem> messages;
        public List<MOBItem> Messages { get { return this.messages; } set { this.messages = value; } }
    }
}
