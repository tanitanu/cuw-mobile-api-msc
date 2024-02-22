using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.TouchlessPayments
{
   
    public class SavePaxWalletResponse
    {
        public string Text { get; set; }
        public string Successful { get; set; }
        public List<CslError> Errors { get; set; }
    }
}
