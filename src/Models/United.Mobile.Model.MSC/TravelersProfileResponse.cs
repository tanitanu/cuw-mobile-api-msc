using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.CSLModels.CustomerProfile
{
    public class TravelersProfileResponse: Base
    {
        public List<TravelerProfileResponse> Travelers
        {
            get;
            set;
        }

    }
}
