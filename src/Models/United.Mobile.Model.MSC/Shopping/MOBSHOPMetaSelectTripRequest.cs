using System;
using United.Mobile.Model.Common;

namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBSHOPMetaSelectTripRequest : MOBRequest
    {
        private string medaSessionId = string.Empty;
        private string cartId = string.Empty;
        private bool requeryForUpsell = false;
        private string bbxSolutionId = string.Empty;
        private string bbxCellId = string.Empty;
        private string mileagePlusAccountNumber;
        private int premierStatusLevel;
        private bool isCatalogOnForTavelerTypes;
        private string flow = string.Empty;
        public string Flow
        {
            get
            {
                return this.flow;
            }
            set
            {
                this.flow = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public bool IsCatalogOnForTavelerTypes
        {
            get { return isCatalogOnForTavelerTypes; }
            set { isCatalogOnForTavelerTypes = value; }
        }

        public string MedaSessionId
        {
            get
            {
                return this.medaSessionId;
            }
            set
            {
                this.medaSessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

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

        public bool RequeryForUpsell
        {
            get { return requeryForUpsell; }
            set { requeryForUpsell = value; }
        }

        public string BbxSolutionId
        {
            get { return bbxSolutionId; }
            set { bbxSolutionId = value; }
        }

        public string BbxCellId
        {
            get { return bbxCellId; }
            set { bbxCellId = value; }
        }

        public string MileagePlusAccountNumber
        {
            get { return mileagePlusAccountNumber; }
            set { mileagePlusAccountNumber = value; }
        }

        public int PremierStatusLevel
        {
            get { return premierStatusLevel; }
            set { premierStatusLevel = value; }
        }
    }
}
