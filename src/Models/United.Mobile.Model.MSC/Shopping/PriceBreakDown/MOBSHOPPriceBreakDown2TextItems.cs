﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Shopping.PriceBreakDown
{
     [Serializable()]
    public class MOBSHOPPriceBreakDown2TextItems : MOBSHOPPriceBreakDown2Items
    {
         private string text2;

         public string Text2
         {
             get
             {
                 return text2;
             }
             set
             {
                 text2 = value;
             }

         }
    }
}
