using System;
using System.Collections.Generic;
namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPRegisterOffer
    {
        private string productCode = string.Empty;
        private string productId = string.Empty;
        private List<string> productIds;
        private string subProductCode = string.Empty;

        public string ProductCode
        {
            get
            {
                return this.productCode;
            }
            set
            {
                this.productCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string ProductId
        {
            get
            {
                return this.productId;
            }
            set
            {
                this.productId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public List<string> ProductIds
        {
            get
            {
                return this.productIds;
            }
            set
            {
                this.productIds = value;
            }
        }

        public string SubProductCode
        {
            get
            {
                return this.subProductCode;
            }
            set
            {
                this.subProductCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }

    [Serializable]
    public class MOBSHOPBundleRegisterOffer
    {
        private string productCode = string.Empty;
        private string productId;
        private List<string> tripIds;
        List<string> selectedTripProductIDs;


        private int productIndex;
        public string ProductCode
        {
            get
            {
                return this.productCode;
            }
            set
            {
                this.productCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

      

        public string ProductId
        {
            get
            {
                return this.productId;
            }
            set
            {
                this.productId = value;
            }
        }

        public List<string> TripIds
        {
            get
            {
                return this.tripIds;
            }
            set
            {
                this.tripIds = value;
            }
        }


        public int ProductIndex
        {
            get
            {
                return this.productIndex;
            }
            set
            {
                this.productIndex = value;
            }
        }
        public List<string> SelectedTripProductIDs
        {
            get
            {
                return this.selectedTripProductIDs;
            }
            set
            {
                this.selectedTripProductIDs = value;
            }
        }
    }
}
