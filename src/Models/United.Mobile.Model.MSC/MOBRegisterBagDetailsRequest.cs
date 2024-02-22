using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition;
using System.Collections.ObjectModel;

namespace United.Mobile
{
    public class MOBRegisterBagDetailsRequest : MOBShoppingRequest
    {
        public Collection<BagDetail> BagDetails { get; set; }
        public string MilitaryTravelType { get; set; }
    }

    public class BagDetail
    {
        public string CustomerId { get; set; }
        public List<BagItem> BagItems { get; set; }
        public int TotalBags { get; set; }
    }

    public class BagItem
    {
        public string Code { get; set; }
        public int Count { get; set; }
        public int MiscCode { get; set; }
        public List<BagAttribute> BagAttributes { get; set; }
    }

    public class BagAttribute
    {
        public bool isOverweight1 { get; set; }
        public bool isOverweight2 { get; set; }
        public bool isOversize { get; set; }
        public bool hasBootItem { get; set; }
    }
}
