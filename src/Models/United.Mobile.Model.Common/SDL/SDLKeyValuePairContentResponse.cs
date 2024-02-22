using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition.SDL;

namespace United.Mobile.Model.Common
{
    public class SDLKeyValuePairContentResponse
    {
        public List<KeyValuePairContent> content { get; set; }
        public List<SDLError> ErrorList { get; set; }
    }

    public class KeyValuePairContent
    {
        public KeyValuePairSubContent content { get; set; }
        public string id { get; set; }
    }

    public class KeyValuePairSubContent
    {
        public List<KeyValuePairLabelContent> labels { get; set; }
    }
    public class KeyValuePairLabelContent
    {
        public KeyValuePair content { get; set; }
    }
}
