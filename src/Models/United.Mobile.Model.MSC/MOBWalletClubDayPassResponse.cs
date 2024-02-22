using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MOBWalletClubDayPassResponse
    {
        private List<MOBClubDayPass> passes;

        public List<MOBClubDayPass> Passes
        {
            get
            {
                return this.passes;
            }
            set
            {
                this.passes = value;
            }
        }
    }
}
