using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using United.Definition.Shopping;

namespace United.Definition
{
    [Serializable()]
    public class MOBRegisterOfferRequest : MOBShoppingRequest
    {
        private MOBSHOPClubPassPurchaseRequest clubPassPurchaseRequest;
        private Collection<MerchandizingOfferDetails> merchandizingOfferDetails;
        private bool continueToRegisterAncillary;
        private bool isOmniCartSavedTripFlow;
        public bool IsOmniCartSavedTripFlow
        {
            get { return isOmniCartSavedTripFlow; }
            set { isOmniCartSavedTripFlow = value; }
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
        public Collection<MerchandizingOfferDetails> MerchandizingOfferDetails
        {
            get
            {
                return this.merchandizingOfferDetails;
            }
            set
            {
                this.merchandizingOfferDetails = value;
            }
        }

        public bool ContinueToRegisterAncillary
        {
            get { return continueToRegisterAncillary; }
            set { continueToRegisterAncillary = value; }
        }
        private bool isRemove;

        public bool IsRemove
        {
            get { return isRemove; }
            set { isRemove = value; }
        }

        private bool isContinue;
        public bool IsContinue
        {
            get { return isContinue; }
            set { isContinue = value; }
        }

    }
    [Serializable()]
    public class MerchandizingOfferDetails
    {
        private string productCode = string.Empty;
        private List<string> productIds = new List<string>();
        private bool isOfferRegistered;
        private string subProductCode = string.Empty;
        private List<string> tripIds;
        List<string> selectedTripProductIDs;
        private bool isReQuote = false;
        private bool isAcceptChanges = false;
        private int numberOfPasses;
        public string ProductCode
        {
            get
            {
                return this.productCode;
            }
            set
            {
                this.productCode = value;
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
        public bool IsOfferRegistered
        {
            get
            {
                return this.isOfferRegistered;
            }
            set
            {
                this.isOfferRegistered = value;
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
                this.subProductCode = value;
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
        public bool IsReQuote
        {
            get
            {
                return this.isReQuote;
            }
            set
            {
                this.isReQuote = value;
            }
        }
        public bool IsAcceptChanges
        {
            get
            {
                return this.isAcceptChanges;
            }
            set
            {
                this.isAcceptChanges = value;
            }
        }
        public int NumberOfPasses
        {
            get
            {
                return this.numberOfPasses;
            }
            set
            {
                this.numberOfPasses = value;
            }
        }
    }
}
