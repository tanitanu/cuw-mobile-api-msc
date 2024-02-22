using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable]
    public class MOBFrequentFlyerRewardProgramsResponse : MOBResponse
    {
        private List<MOBSHOPRewardProgram> rewardProgramList;

        public List<MOBSHOPRewardProgram> RewardProgramList
        {
            get
            {
                return this.rewardProgramList;
            }
            set
            {
                this.rewardProgramList = value;
            }
        }
    }
}
