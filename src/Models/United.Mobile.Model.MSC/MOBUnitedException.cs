using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Runtime.Serialization;

namespace United.Definition
{
    [Serializable()]
    public class MOBUnitedException : System.Exception
    {
        private string code = string.Empty;

        public string Code
        {
            get; set;
        }
        public MOBUnitedException()
        {
        }

        public MOBUnitedException(string code, string message) : base(message)
        {
            this.Code = code;
            //base.Message = message;
        }

        public MOBUnitedException(string message)
            : base(message)
        {
        }

        public MOBUnitedException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }

        protected MOBUnitedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

    }
}
