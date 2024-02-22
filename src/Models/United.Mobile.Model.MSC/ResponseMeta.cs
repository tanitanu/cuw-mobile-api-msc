using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.CSLModels.CustomerProfile
{
    public class ResponseMeta
    {
        public List<ResponseTime> ResponseTimes { get; set; }

        public List<Error> PartialErrors { get; set; }
    }
}
