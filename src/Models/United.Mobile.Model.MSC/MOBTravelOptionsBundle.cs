﻿using System;
using System.Collections.Generic;
using United.Definition;
using United.Definition.Shopping;
using United.Definition.Shopping.Bundles;
using United.Mobile.Model.Common;

namespace United.Mobile.Model.ManageRes
{
    [Serializable()]
    public class MOBTravelOptionsBundle
    {
        private MOBOfferTile offerTile;
        private List<MOBBundleProduct> products;
        private MOBMobileCMSContentMessages termsAndCondition;
        private int numberOfTravelers;

        public MOBOfferTile OfferTile
        {
            get { return offerTile; }
            set { offerTile = value; }
        }

        public List<MOBBundleProduct> Products
        {
            get { return products; }
            set { products = value; }
        }

        public MOBMobileCMSContentMessages TermsAndCondition
        {
            get { return termsAndCondition; }
            set { termsAndCondition = value; }
        }

        public int NumberOfTravelers
        {
            get { return numberOfTravelers; }
            set { numberOfTravelers = value; }
        }


    }
}