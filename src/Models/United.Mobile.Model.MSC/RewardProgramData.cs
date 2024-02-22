using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.CSLModels.CustomerProfile
{
    public class RewardProgramData
    {
       
        public int ProgramId
        {
            get;
            set;
        }

      
        public string ProgramMemberId
        {
            get;
            set;
        }
        public string Description
        {
            get;
            set;
        }
        public string VendorCode
        {
            get;
            set;
        }
        public string VendorDescription { get; set; }
        public string Key { set; get; }
    }
}
