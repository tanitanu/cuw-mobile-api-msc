using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable]
    public class MOBKey
    {
        private int index;

        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        private string key;

        public string Key
        {
            get { return key; }
            set { key = value; }
        }

        private string val;

        public string Val
        {
            get { return val; }
            set { val = value; }
        }

    }
}
