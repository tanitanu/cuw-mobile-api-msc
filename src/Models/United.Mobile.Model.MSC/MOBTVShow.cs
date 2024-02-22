using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MOBTVShow
    {
        private string showName = string.Empty;
        private string showDescription = string.Empty;
        private string showTime = string.Empty;
        private string rating = string.Empty;
        private string duration = string.Empty;

        public string ShowName
        {
            get
            {
                return this.showName;
            }
            set
            {
                this.showName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ShowDescription
        {
            get
            {
                return this.showDescription;
            }
            set
            {
                this.showDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ShowTime
        {
            get
            {
                return this.showTime;
            }
            set
            {
                this.showTime = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Rating
        {
            get
            {
                return this.rating;
            }
            set
            {
                this.rating = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Duration
        {
            get
            {
                return this.duration;
            }
            set
            {
                this.duration = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
