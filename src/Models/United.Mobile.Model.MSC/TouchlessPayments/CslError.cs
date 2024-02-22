using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.TouchlessPayments
{
    public class CslError
    {
        public string MajorCode { get; set; }
        public string MinorCode { get; set; }
        public string MajorDescription { get; set; }
        public string MinorDescription { get; set; }
        public string Message { get; set; }
    }
}
