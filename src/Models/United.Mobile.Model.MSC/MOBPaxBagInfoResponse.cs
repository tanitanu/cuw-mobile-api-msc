using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBPaxBagInfoResponse : MOBResponse
    {
        private List<MOBPaxBagInfo> paxBagInfos;

        public MOBPaxBagInfoResponse()
            : base()
        {

        }

        public List<MOBPaxBagInfo> PaxBagInfos
        {
            get
            {
                return this.paxBagInfos;
            }
            set
            {
                this.paxBagInfos = value;
            }
        }
    }
}
