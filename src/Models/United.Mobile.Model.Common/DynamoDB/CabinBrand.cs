using System;
using System.Collections.Generic;
using System.Text;

namespace United.Mobile.Model.DynamoDb.Common
{
    [Serializable]
    public class CabinBrand
    {
        public int SecondCabinBrandingId { get; set; }
        public int ThirdCabinBrandingId { get; set; }
    }
}
