using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.MPPINPWD;

namespace United.Definition
{
    [Serializable()]
    public class MOBCPCubaSSR
    {
        private string code;
        public string Code
        {
            get { return code; }
            set { code = value; }
        }

        private string vanity;
        public string Vanity
        {
            get { return vanity; }
            set { vanity = value; }
        }

        private string key;
        public string Key
        {
            get { return key; }
            set { key = value; }
        }

        private string inputValue;
        public string InputValue
        {
            get { return inputValue; }
            set { inputValue = value; }
        }



    }
}

