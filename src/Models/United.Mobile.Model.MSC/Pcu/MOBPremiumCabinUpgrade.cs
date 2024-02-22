using System;
using System.Collections.Generic;
using United.Definition.Shopping;
using United.Mobile.Model.Common;
namespace United.Definition.Pcu
{
    [Serializable()]
    public class MOBPremiumCabinUpgrade
    {
        private string productCode = string.Empty;
        private string productName = string.Empty;
        private MOBOfferTile offerTile;
        private MOBPcuOptions pcuOptions;
        private List<MOBItem> captions;
        private List<MOBMobileCMSContentMessages> mobileCmsContentMessages;
        private string pkDispenserPublicKey;
        private string cartId;

        public string ProductCode
        {
            get { return productCode; }
            set { productCode = value; }
        }
        public string ProductName
        {
            get { return productName; }
            set { productName = value; }
        }
        public MOBOfferTile OfferTile
        {
            get { return offerTile; }
            set { offerTile = value; }
        }

        public MOBPcuOptions PcuOptions
        {
            get { return pcuOptions; }
            set { pcuOptions = value; }
        }

        public List<MOBItem> Captions
        {
            get { return captions; }
            set { captions = value; }
        }

        public List<MOBMobileCMSContentMessages> MobileCmsContentMessages
        {
            get { return mobileCmsContentMessages; }
            set { mobileCmsContentMessages = value; }
        }

        public string PkDispenserPublicKey
        {
            get { return pkDispenserPublicKey; }
            set { pkDispenserPublicKey = value; }
        }

        public string CartId
        {
            get { return cartId; }
            set { cartId = value; }
        }
    }
}
