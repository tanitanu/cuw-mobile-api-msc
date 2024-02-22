using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPShoppingProductDetail
    {
        private string title = string.Empty;
        public string Title
        {
            get { return title; }
            set { title = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string header = string.Empty;
        public string Header
        {
            get { return header; }
            set { header = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string body = string.Empty;
        public string Body
        {
            get { return body; }
            set { body = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private List<string> productDetails;
        public List<string> ProductDetails
        {
            get { return productDetails; }
            set { productDetails = value; }
        }

        private List<MOBShopProductDetails> productDetailsFsrRedesign;
        public List<MOBShopProductDetails> ProductDetailsFsrRedesign
        {
            get { return productDetailsFsrRedesign; }
            set { productDetailsFsrRedesign = value; }
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

        private List<MOBSHOPShoppingProductDetailCabinMessage> productCabinMessages;
        public List<MOBSHOPShoppingProductDetailCabinMessage> ProductCabinMessages
        {
            get { return productCabinMessages; }
            set { productCabinMessages = value; }
        }
    }

    [Serializable]
    public class MOBShopProductDetails
    {
        private bool isEligible;
        private string option;

        public bool IsEligible
        {
            get { return isEligible; }
            set { isEligible = value; }
        }

        public string Option
        {
            get { return option; }
            set { option = value; }
        }
    }
}
