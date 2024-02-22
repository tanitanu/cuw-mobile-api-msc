﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Pcu
{
    [Serializable()]
    public class MOBPcuUpgradeOption
    {
        private string optionId;
        private string upgradeOptionDescription;
        private string cabinDescriptionForPaymentPage;
        private string formattedPrice;
        private double price;
        private double totalPriceForAllTravelers;
        private List<string> productIds;

        public string OptionId
        {
            get { return optionId; }
            set { optionId = value; }
        }

        public string UpgradeOptionDescription
        {
            get { return upgradeOptionDescription; }
            set { upgradeOptionDescription = value; }
        }

        public string CabinDescriptionForPaymentPage
        {
            get { return cabinDescriptionForPaymentPage; }
            set { cabinDescriptionForPaymentPage = value; }
        }

        public string FormattedPrice
        {
            get { return formattedPrice; }
            set { formattedPrice = value; }
        }

        public double Price
        {
            get { return price; }
            set { price = value; }
        }

        public double TotalPriceForAllTravelers
        {
            get { return totalPriceForAllTravelers; }
            set { totalPriceForAllTravelers = value; }
        }

        public List<string> ProductIds
        {
            get { return productIds; }
            set { productIds = value; }
        }
    }
}
