using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MOBTVChannel
    {
        private string channelName = string.Empty;
        private string channelCallSign = string.Empty;
        List<MOBTVShow> tvShows;

        public string ChannelName
        {
            get
            {
                return this.channelName;
            }
            set
            {
                this.channelName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ChannelCallSign
        {
            get
            {
                return this.channelCallSign;
            }
            set
            {
                this.channelCallSign = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<MOBTVShow> TVShows
        {
            get
            {
                return this.tvShows;
            }
            set
            {
                this.tvShows = value;
            }
        }
    }
}
