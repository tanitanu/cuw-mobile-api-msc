using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPShoppingProductDetailCabinMessage
    {


        private string segments = string.Empty;
        public string Segemnts
        {
            get { return segments; }
            set { segments = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string cabin = string.Empty;
        public string Cabin
        {
            get { return cabin; }
            set { cabin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private bool isMixedCabin = false;
        public bool IsMixedCabin
        {
            get { return isMixedCabin; }
            set { isMixedCabin = value; }
        }
    }
}
