﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Accelerators
{
    [Serializable()]
    public class MOBPremierAcceleratorOffer
    {
        private double price;
        private string productCode;
        private string productId;
        private string premierMilesOffer;
        private string subProductCode;

        public double Price
        {
            get { return price; }
            set { price = value; }
        }

        public string ProductCode
        {
            get { return productCode; }
            set { productCode = value; }
        }

        public string ProductId
        {
            get { return productId; }
            set { productId = value; }
        }

        public string PremierMilesOffer
        {
            get { return premierMilesOffer; }
            set { premierMilesOffer = value; }
        }

        public string SubProductCode
        {
            get { return subProductCode; }
            set { subProductCode = value; }
        }
    }
}
