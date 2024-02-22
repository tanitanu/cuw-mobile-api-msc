using System;
using System.Collections.ObjectModel;

namespace United.Definition
{
    public class MOBRegisterBagsRequest : MOBShoppingRequest
    {
        public Collection<MOBBag> Bags { get; set; }
        public string MilitaryTravelType { get; set; }
    }

    public class SpecialtyItem
    {
        public string Code { get; set; }
        public int Count { get; set; }
        public int MiscCode { get; set; }
    }

    public class MOBBag
    {
        public string CustomerId { get; set; }
        public int TotalBags { get; set; }
        public int OverWeightBags { get; set; }
        public int OverWeight2Bags { get; set; }
        public int OverSizeBags { get; set; }
        public int ExceptionItemInfantSeat { get; set; }
        public int ExceptionItemInfantStroller { get; set; }
        public int ExceptionItemWheelChair { get; set; }
        public int ExceptionItemSkiBoots { get; set; }
        public int ExceptionItemHockeySticks { get; set; }
        public int ExceptionFreeCheckedCarryon { get; set; }
        public Collection<SpecialtyItem> SpecialtyItems { get; set; }
    }
}
