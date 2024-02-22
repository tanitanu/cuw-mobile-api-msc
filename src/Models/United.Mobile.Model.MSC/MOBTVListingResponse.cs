using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBTVListingResponse : MOBResponse 
    {
        private List<MOBTVChannel> tvChannels;

        public MOBTVListingResponse()
            : base()
        {
        }

        public List<MOBTVChannel> TVChannels
        {
            get
            {
                return this.tvChannels;
            }
            set
            {
                this.tvChannels = value;
            }
        }        
    }
}
