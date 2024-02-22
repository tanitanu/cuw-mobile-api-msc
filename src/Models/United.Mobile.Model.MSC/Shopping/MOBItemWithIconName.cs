using System;

namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBItemWithIconName
    {
        private string optionDescription = string.Empty;
        private string optionIcon = string.Empty;

        public string OptionDescription
        {
            get
            {
                return this.optionDescription;
            }
            set
            {
                this.optionDescription = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string OptionIcon
        {
            get
            {
                return this.optionIcon;
            }
            set
            {
                this.optionIcon = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

    }
}