using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBBagHistoryResponse : MOBResponse
    {
        public MOBBagHistoryResponse()
            : base()
        {

        }

        private MOBBagTagHistory bagTagHistory;
        public MOBBagTagHistory BagTagHistory
        {
            get
            {
                return this.bagTagHistory;
            }
            set
            {
                this.bagTagHistory = value;
            }
        }
    }
}
