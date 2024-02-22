using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MOBFlifoScheduleError
    {
        public string Message = string.Empty;

        public string Name = string.Empty;

        ///// <remarks/>
        //[System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
        //public string Message
        //{
        //    get
        //    {
        //        return this.messageField;
        //    }
        //    set
        //    {
        //        this.messageField = value;
        //    }
        //}

        ///// <remarks/>
        //[System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
        //public string Name
        //{
        //    get
        //    {
        //        return this.nameField;
        //    }
        //    set
        //    {
        //        this.nameField = value;
        //    }
        //}
    }
}
