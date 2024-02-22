using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp.Shopping
{
    [Serializable]
    public class MOBEmpSHOPShoppingProduct
    {
        private string productId = string.Empty;
        public string ProductId
        {
            get { return productId; }
            set { productId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string type = string.Empty;
        public string Type
        {
            get { return type; }
            set { type = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string subType = string.Empty;
        public string SubType
        {
            get { return subType; }
            set { subType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string cabin = string.Empty;
        public string Cabin
        {
            get { return cabin; }
            set { cabin = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string description = string.Empty;
        public string Description
        {
            get { return description; }
            set { description = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string price = string.Empty;
        public string Price
        {
            get { return price; }
            set { price = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string milesDisplayValue = string.Empty;
        public string MilesDisplayValue
        {
            get { return milesDisplayValue; }
            set { milesDisplayValue = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private decimal priceAmount = 0;
        public decimal PriceAmount
        {
            get { return priceAmount; }
            set { priceAmount = value; }
        }

        private decimal milesDisplayAmount = 0;
        public decimal MilesDisplayAmount
        {
            get { return milesDisplayAmount; }
            set { milesDisplayAmount = value; }
        }

        private string meal = string.Empty;
        public string Meal
        {
            get { return meal; }
            set { meal = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private bool isLowest = false;
        public bool IsLowest
        {
            get { return isLowest; }
            set { isLowest = value; }
        }

        private MOBEmpSHOPShoppingProductDetail productDetail;
        public MOBEmpSHOPShoppingProductDetail ProductDetail
        {
            get { return productDetail; }
            set { productDetail = value; }
        }

        private bool isMixedCabin = false;
        public bool IsMixedCabin
        {
            get { return isMixedCabin; }
            set { isMixedCabin = value; }
        }

        private List<string> mixedCabinSegmentMessages;
        public List<string> MixedCabinSegmentMessages
        {
            get { return mixedCabinSegmentMessages; }
            set { mixedCabinSegmentMessages = value; }
        }

        private string awardType = string.Empty;
        public string AwardType
        {
            get { return awardType; }
            set { awardType = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string allCabinButtonText = string.Empty;
        public string AllCabinButtonText
        {
            get { return allCabinButtonText; }
            set { allCabinButtonText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private bool isSelectedCabin = false;
        public bool IsSelectedCabin
        {
            get { return isSelectedCabin; }
            set { isSelectedCabin = value; }
        }

        private int mileageButton = -1;
        public int MileageButton
        {
            get { return mileageButton; }
            set { mileageButton = value; }
        }

        private int seatsRemaining = -1;
        public int SeatsRemaining
        {
            get { return seatsRemaining; }
            set { seatsRemaining = value; }
        }
        
        private string pqdText = string.Empty;
        public string PqdText
        {
            get { return pqdText; }
            set { pqdText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string pqmText = string.Empty;
        public string PqmText
        {
            get { return pqmText; }
            set { pqmText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string rdmText = string.Empty;
        public string RdmText
        {
            get { return rdmText; }
            set { rdmText = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private bool isPremierCabinSaver = false;
        public bool ISPremierCabinSaver
        {
            get { return isPremierCabinSaver; }
            set { isPremierCabinSaver = value; }
        }

    }
}
