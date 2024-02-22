﻿using System;
using System.Configuration;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping.Bundles
{
    [Serializable]
    public class MOBBookingBundlesResponse : MOBResponse
    {
        [JsonIgnore()]
        public string ObjectName { get; set; } = "United.Definition.Shopping.Bundles.MOBBookingBundlesResponse";
        private readonly IConfiguration _configuration;
        private int totalAmount;
        private List<MOBBundleProduct> products;
        private string bookingBundlesTitle = "Traveler Information" ?? string.Empty;
        private MOBMobileCMSContentMessages termsAndCondition;
        private string flow;
        private string sessionId;
        private string cartId;
        // MOBILE-25395: SAF
        private MOBMobileCMSContentMessages additionalTermsAndCondition;

        public MOBBookingBundlesResponse() // don't remove empty ctor. this required for json deserialize object using GetSessionMethod
        {

        }
        public MOBBookingBundlesResponse(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public List<MOBBundleProduct> Products
        {
            get { return products; }
            set
            {
                products = value ?? new List<MOBBundleProduct>();
                
                if (_configuration != null)
                {
                    if (products != null && products.Count > 0)
                    {
                        bookingBundlesTitle = String.IsNullOrEmpty(_configuration.GetValue<string>("BookingBundlesScreenTitle")) ? "Customize Your Itinerary" : _configuration.GetValue<string>("BookingBundlesScreenTitle");
                    }
                    else
                    {
                        bookingBundlesTitle = String.IsNullOrEmpty(_configuration.GetValue<string>("BookingBundlesNoBundlesScreenTitle")) ? "Traveler Information" : _configuration.GetValue<string>("BookingBundlesNoBundlesScreenTitle");
                    }
                }
                else
                {
                    // Code added just for safer side to not break anything
                    if (products != null && products.Count > 0)
                    {
                        bookingBundlesTitle = "Customize Your Itinerary";
                    }
                    else
                    {
                        bookingBundlesTitle = "Traveler Information";
                    }
                }
            }
        }

        public string BookingBundlesTitle
        {
            get
            {
                if (_configuration != null)
                {
                    if (products != null && products.Count > 0)
                    {
                        bookingBundlesTitle = String.IsNullOrEmpty(_configuration.GetValue<string>("BookingBundlesScreenTitle")) ? "Customize Your Itinerary" : _configuration.GetValue<string>("BookingBundlesScreenTitle");
                    }
                    else
                    {
                        bookingBundlesTitle = String.IsNullOrEmpty(_configuration.GetValue<string>("BookingBundlesNoBundlesScreenTitle")) ? "Traveler Information" : _configuration.GetValue<string>("BookingBundlesNoBundlesScreenTitle");
                    }
                }
                else
                {
                    // Code added just for safer side to not break anything
                    if (products != null && products.Count > 0)
                    {
                        bookingBundlesTitle = "Customize Your Itinerary";
                    }
                    else
                    {
                        bookingBundlesTitle = "Traveler Information";
                    }
                }
                return bookingBundlesTitle;
            }
            set { bookingBundlesTitle = value; }
        }

        public MOBMobileCMSContentMessages TermsAndCondition
        {
            get { return termsAndCondition; }
            set { termsAndCondition = value; }
        }

        // MOBILE-25395: SAF
        public MOBMobileCMSContentMessages AdditionalTermsAndCondition
        {
            get { return additionalTermsAndCondition; }
            set { additionalTermsAndCondition = value; }
        }

        public int TotalAmount
        {
            get { return totalAmount; }
            set { totalAmount = value; }
        }
        public string Flow
        {
            get { return flow; }
            set { flow = value; }
        }
        public string SessionId
        {
            get { return sessionId; }
            set { sessionId = value; }
        }
        public string CartId
        {
            get { return cartId; }
            set { cartId = value; }
        }

        private string clearOption = string.Empty;
        public string ClearOption
        {
            get { return this.clearOption; }
            set { clearOption = value; }
        }
    }

    #region NewBundles
    [Serializable]
    public class MOBBundleProduct
    {
        private string productID;
        private string productCode;
        private List<string> productIDs;
        private string productName;
        private MOBBundleTile tile;
        private MOBBundleDetail detail;
        private int amount;
        private int productIndex;
        private List<string> bundleProductCodes;

        public List<string> BundleProductCodes
        {
            get { return bundleProductCodes; }
            set { bundleProductCodes = value; }
        }

        public string ProductID
        {
            get { return productID; }
            set { productID = value; }
        }
        public List<string> ProductIDs
        {
            get { return productIDs; }
            set { productIDs = value; }
        }
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

        public MOBBundleTile Tile
        {
            get { return tile; }
            set { tile = value; }
        }

        public MOBBundleDetail Detail
        {
            get { return detail; }
            set { detail = value; }
        }

        public int Amount
        {
            get { return amount; }
            set { amount = value; }
        }

        public int ProductIndex
        {
            get { return productIndex; }
            set { productIndex = value; }
        }

    }

    [Serializable]
    public class MOBBundleTile
    {
        private string offerTitle;
        private List<string> offerDescription;
        private string priceText;
        private bool isSelected;
        private int tileIndex;
        private string offerPrice;
        private string bundleBadgeText;
        private string fromText;

        public string BundleBadgeText
        {
            get { return bundleBadgeText; }
            set { bundleBadgeText = value; }
        }

        public string OfferTitle
        {
            get { return offerTitle; }
            set { offerTitle = value; }
        }

        public List<string> OfferDescription
        {
            get { return offerDescription; }
            set { offerDescription = value; }
        }

        public string PriceText
        {
            get { return priceText; }
            set { priceText = value; }
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; }
        }

        public int TileIndex
        {
            get { return tileIndex; }
            set { tileIndex = value; }
        }
        public string OfferPrice
        {
            get { return offerPrice; }
            set { offerPrice = value; }
        }
        public string FromText
        {
            get { return fromText; }
            set { fromText = value; }
        }
    }

    [Serializable]
    public class MOBBundleDetail
    {
        private string offerTitle;
        private string offerWarningMessage;
        private List<MOBBundleOfferDetail> offerDetails;
        private List<MOBBundleOfferTrip> offerTrips;
        private double incrementSliderValue;

        public string OfferTitle
        {
            get { return offerTitle; }
            set { offerTitle = value; }
        }
        public string OfferWarningMessage
        {
            get { return offerWarningMessage; }
            set { offerWarningMessage = value; }
        }

        public List<MOBBundleOfferDetail> OfferDetails
        {
            get { return offerDetails; }
            set { offerDetails = value; }
        }

        public List<MOBBundleOfferTrip> OfferTrips
        {
            get { return offerTrips; }
            set { offerTrips = value; }
        }
        public double IncrementSliderValue
        {
            get { return incrementSliderValue; }
            set { incrementSliderValue = value; }
        }
    }

    [Serializable]
    public class MOBBundleOfferDetail
    {
        private string offerDetailHeader;
        private string offerDetailDescription;
        private string offerDetailWarningMessage;

        public string OfferDetailHeader
        {
            get { return offerDetailHeader; }
            set { offerDetailHeader = value; }
        }

        public string OfferDetailDescription
        {
            get { return offerDetailDescription; }
            set { offerDetailDescription = value; }
        }

        public string OfferDetailWarningMessage
        {
            get { return offerDetailWarningMessage; }
            set { offerDetailWarningMessage = value; }
        }

    }

    [Serializable]
    public class MOBBundleOfferTrip
    {
        private string originDestination;
        private string tripId;
        private bool isChecked;
        private int price;
        private string tripProductID;
        private List<string> tripProductIDs;
        private double amount;
        private bool isDefault;

        public string OriginDestination
        {
            get { return originDestination; }
            set { originDestination = value; }
        }

        public string TripId
        {
            get { return tripId; }
            set { tripId = value; }
        }

        public bool IsChecked
        {
            get { return isChecked; }
            set { isChecked = value; }
        }

        public int Price
        {
            get { return price; }
            set { price = value; }
        }
        public string TripProductID
        {
            get { return tripProductID; }
            set { tripProductID = value; }
        }
        public List<string> TripProductIDs
        {
            get { return tripProductIDs; }
            set { tripProductIDs = value; }
        }
        public double Amount
        {
            get { return amount; }
            set { amount = value; }
        }
        public bool IsDefault
        {
            get { return isDefault; }
            set { isDefault = value; }
        }
    }

    #endregion




}
