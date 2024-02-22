using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.MSC
{
    public class PKDispenserResponse
    {
        //public PKDispenserResponse();

        public List<Key> Keys { get; set; }
        public PKDispenserResponse()
        {
            Keys = new List<Key>();
        }
    }
}
