using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable]
    public class MOBSHOPRegisterOfferRequest : MOBRequest
    {
        private string cartId = string.Empty;
        private string cartKey = string.Empty;
        private string sessionId = string.Empty;
        private string pointOfSale = string.Empty;
        private List<MOBSHOPRegisterOffer> offers;
        private MOBSHOPClubPassPurchaseRequest clubPassPurchaseRequest;
        private List<MOBSHOPBundleRegisterOffer> products;
        private List<MOBSHOPBundleRegisterOffer> productsToBeRemoved;

        public string CartId
        {
            get
            {
                return this.cartId;
            }
            set
            {
                this.cartId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string CartKey
        {
            get
            {
                return this.cartKey;
            }
            set
            {
                this.cartKey = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string SessionId
        {
            get
            {
                return this.sessionId;
            }
            set
            {
                this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string PointOfSale
        {
            get
            {
                return this.pointOfSale;
            }
            set
            {
                this.pointOfSale = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public List<MOBSHOPRegisterOffer> Offers
        {
            get
            {
                return this.offers;
            }
            set
            {
                this.offers = value;
            }
        }

        public MOBSHOPClubPassPurchaseRequest ClubPassPurchaseRequest
        {
            get
            {
                return this.clubPassPurchaseRequest;
            }
            set
            {
                this.clubPassPurchaseRequest = value;
            }
        }


        public List<MOBSHOPBundleRegisterOffer> Products
        {
            get
            {
                return this.products;
            }
            set
            {
                this.products = value;
            }
        }

        public List<MOBSHOPBundleRegisterOffer> ProductsToBeRemoved
        {
            get
            {
                return this.productsToBeRemoved;
            }
            set
            {
                this.productsToBeRemoved = value;
            }
        }
    }
}
