using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable]
    public class MOBSection
    {
        private string text1;
        private string text2;
        private string text3;
        private string messageType;
        private bool isDefaultOpen = true;
        private string order;

        public string Order
        {
            get { return order; }
            set { order = value; }
        }


        public string Text1
        {
            get
            {
                return this.text1;
            }
            set
            {
                this.text1 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string Text2
        {
            get
            {
                return this.text2;
            }
            set
            {
                this.text2 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string Text3
        {
            get
            {
                return this.text3;
            }
            set
            {
                this.text3 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string MessageType
        {
            get
            {
                return this.messageType;
            }
            set
            {
                this.messageType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool IsDefaultOpen
        {
            get
            {
                return this.isDefaultOpen;
            }
            set
            {
                this.isDefaultOpen = value;
            }
        }
    }
}
