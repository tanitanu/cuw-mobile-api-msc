using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;

namespace United.Definition
{
    [Serializable()]
    public class MOBTNCItem : MOBSection
    {
        private List<MOBMobileCMSContentMessages> tnCList;

        public List<MOBMobileCMSContentMessages> TnCList
        {
            get { return tnCList; }
            set { tnCList = value; }
        }
    }
}
