using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using United.Definition.Shopping;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBBasicEconomyBuyOut
    {
        private string productCode;
        private MOBItem elfRestrictionsBeBuyOutLink;
        private List<string> productIds;
        private List<MOBBeBuyOutSegment> productDetail;
        private List<MOBItem> captions;
        private MOBMobileCMSContentMessages termsAndConditions;
        private List<MOBItem> faqs;
        private MOBBeBuyOutOffer offerTile;

        public string ProductCode
        {
            get { return productCode; }
            set { productCode = value; }
        }

        public MOBItem ElfRestrictionsBeBuyOutLink
        {
            get { return elfRestrictionsBeBuyOutLink; }
            set { elfRestrictionsBeBuyOutLink = value; }
        }

        public List<string> ProductIds
        {
            get { return productIds; }
            set { productIds = value; }
        }

        public List<MOBItem> Captions
        {
            get { return captions; }
            set { captions = value; }
        }
        [JsonProperty(PropertyName = "termsAndConditions")]
        [JsonPropertyName("termsAndConditions")]
        public MOBMobileCMSContentMessages MobileCmsContentMessages
        {
            get { return termsAndConditions; }
            set { termsAndConditions = value; }
        }

        public List<MOBItem> Faqs
        {
            get { return faqs; }
            set { faqs = value; }
        }

        public List<MOBBeBuyOutSegment> ProductDetail
        {
            get { return productDetail; }
            set { productDetail = value; }
        }

        public MOBBeBuyOutOffer OfferTile
        {
            get { return offerTile; }
            set { offerTile = value; }
        }
    }

    [Serializable()]
    public class MOBBeBuyOutSegment
    {
        private string segment;
        private string message;
        private string warningIcon;
        private bool warning;

        public string Segment
        {
            get { return segment; }
            set { segment = value; }
        }

        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        public string WarningIcon
        {
            get { return warningIcon; }
            set { warningIcon = value; }
        }

        public bool Warning
        {
            get { return warning; }
            set { warning = value; }
        }
    }

    [Serializable()]
    public class MOBBeBuyOutOffer
    {
        private string title;
        private string header;
        private List<MOBSHOPOption> elfShopOptions;
        private decimal price;
        private string currencyCode;
        private string text1;
        private string text2;
        private string button;


        public string Title
        {
            get { return title; }
            set { title = value; }
        }

        public string Header
        {
            get { return header; }
            set { header = value; }
        }

        public List<MOBSHOPOption> ELFShopOptions
        {
            get
            {
                return this.elfShopOptions;
            }
            set
            {
                this.elfShopOptions = value;
            }
        }

        public decimal Price
        {
            get { return price; }
            set { price = value; }
        }

        public string Text1
        {
            get { return text1; }
            set { text1 = value; }
        }

        public string Text2
        {
            get { return text2; }
            set { text2 = value; }
        }

        public string Button
        {
            get { return button; }
            set { button = value; }
        }

        public string CurrencyCode
        {
            get { return currencyCode; }
            set { currencyCode = value; }
        }
    }
}