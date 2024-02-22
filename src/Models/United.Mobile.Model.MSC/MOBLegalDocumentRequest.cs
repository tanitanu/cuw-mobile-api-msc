using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{

    [Serializable]
    public class MOBLegalDocumentRequest : MOBRequest
    {
        private List<string> titles;

        public List<string> Titles
        {
            get
            {
                return this.titles;
            }
            set
            {
                this.titles = value;
            }
        }
    }
}
