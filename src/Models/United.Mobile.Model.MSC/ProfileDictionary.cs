using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.CSLModels.CustomerProfile
{
    public class ProfileDictionary
    {
        #region Constructors

        public ProfileDictionary(string key, string value)
        {
            Key = key;
            Value = value;
        }

        #endregion Constructors

        #region Properties
     
        public string Key { get; set; }    
        public string Value { get; set; }

        #endregion Properties
    }
}
