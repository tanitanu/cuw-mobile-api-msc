using System;
using United.Definition.Shopping;

namespace United.Definition.Common
{
    [Serializable]
    public class MOBItemWithIconAndLink : MOBItemWithIconName
    {
        private string linkUrl;

        public string LinkUrl
        {
            get { return linkUrl; }
            set { linkUrl = value; }
        }
    }
}
