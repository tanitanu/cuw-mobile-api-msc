using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPFareRuleList
    {
        private string textType;
        private string ruleText;

        public string TextType
        {
            get
            {
                return this.textType;
            }
            set
            {
                this.textType = value;
            }
        }

        public string RuleText
        {
            get
            {
                return this.ruleText;
            }
            set
            {
                this.ruleText = value;
            }
        }
    }
}
