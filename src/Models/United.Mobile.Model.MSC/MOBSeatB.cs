using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MOBSeatB
    {
        public MOBSeatB() { }

        public MOBSeatB(string number, string seatvalue, string fee, bool exit, bool limitedRecline, bool isEPlus)
        {
            Number = number;
            SeatValue = seatvalue;
            Fee = fee;
            Exit = exit;
            LimitedRecline = limitedRecline;
            IsEPlus = isEPlus;
        }

        private string displaySeatFeature = string.Empty;
        public string DisplaySeatFeature
        {
            get
            {
                return this.displaySeatFeature;
            }
            set
            {
                this.displaySeatFeature = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        private bool isHighestEPlus;
        public bool IsHighestEPlus
        {
            get { return isHighestEPlus; }
            set { isHighestEPlus = value; }
        }

        private bool isLowestEPlus;
        public bool IsLowestEPlus
        {
            get { return isLowestEPlus; }
            set { isLowestEPlus = value; }
        }

        private List<MOBServicesAndFees> servicesAndFees;
        public List<MOBServicesAndFees> ServicesAndFees
        {
            get { return servicesAndFees; }
            set { servicesAndFees = value; }
        }

        private string number;
        public string Number
        {
            get { return number; }
            set { number = value; }
        }

        public string seatvalue;
        public string SeatValue
        {
            get { return seatvalue; }
            set { seatvalue = value; }
        }

        private string fee;
        public string Fee
        {
            get { return fee; }
            set { fee = value; }
        }

        private bool exit;
        public bool Exit
        {
            get { return exit; }
            set { exit = value; }
        }
        private bool limitedRecline;
        public bool LimitedRecline
        {
            get { return limitedRecline; }
            set { limitedRecline = value; }
        }
        private bool isEPlus;
        public bool IsEPlus
        {
            get { return isEPlus; }
            set { isEPlus = value; }
        }

        private string program;
        public string Program
        {
            get
            {
                return this.program;
            }
            set
            {
                this.program = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        private List<MOBCharacteristic> characteristics;
        public List<MOBCharacteristic> Characteristics
        {
            get { return this.characteristics; }
            set { this.characteristics = value; }
        }

        private string seatFeatureInfo;       

        public string SeatFeatureInfo
        {
            get { return seatFeatureInfo; }
            set { seatFeatureInfo = value; }
        }      

        private List<string> seatFeatureList;

        public List<string> SeatFeatureList
        {
            get { return seatFeatureList; }
            set { seatFeatureList = value; }
        }

        private bool isPcuOfferEligible;

        public bool IsPcuOfferEligible
        {
            get { return isPcuOfferEligible; }
            set { isPcuOfferEligible = value; }
        }

        private string pcuOfferPrice;   

        public string PcuOfferPrice
        {
            get { return pcuOfferPrice; }
            set { pcuOfferPrice = value; }
        }

        private string pcuOfferOptionId;

        public string PcuOfferOptionId
        {
            get { return pcuOfferOptionId; }
            set { pcuOfferOptionId = value; }
        }
    }
}
