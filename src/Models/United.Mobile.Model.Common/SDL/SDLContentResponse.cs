using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.SDL
{
    public class SDLContentResponse
    {
        public List<Content> content { get; set; }
        public List<SDLError> ErrorList { get; set; }
    }
    public class Content
    {
        public SubContent content { get; set; }
        public string id { get; set; }
       
    }
    public class SubContent
    {     
        public string title { get; set; }
        public string component_key { get; set; }
        public string body { get; set; }
    }
    public class SDLError
    {
        public string Code { get; set; }
        public string FieldName { get; set; }
        public string Message { get; set; }
    }

}
