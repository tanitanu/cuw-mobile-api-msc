using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.Model.MSC
{
    [Serializable()]
    public class MOBExtraSeat
    {
        private int selectedPaxId;

        public int SelectedPaxId
        {
            get { return selectedPaxId; }
            set { selectedPaxId = value; }
        }

        private string passengerDesc;

        public string PassengerDesc
        {
            get { return passengerDesc; }
            set { passengerDesc = value; }
        }


        private string purpose;

        public string Purpose
        {
            get { return purpose; }
            set { purpose = value; }
        }

        private bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; }
        }

        public enum EXTRASEATCOUNTFORSSRREMARKS_PERSONAL_COMFORT
        {
            [Description("Extra Seat")]
            EXST = 0,
            [Description("Extra Seat")]
            EXSTTWO = 1,
            [Description("Extra Seat")]
            EXSTTHREE = 2,
            [Description("Extra Seat")]
            EXSTFOUR = 3,
            [Description("Extra Seat")]
            EXSTFIVE = 4,
            [Description("Extra Seat")]
            EXSTSIX = 5,
            [Description("Extra Seat")]
            EXSTSEVEN = 6
        }

        public enum EXTRASEATCOUNTFORSSRREMARKS_CABIN_BAGGAGE
        {
            [Description("Extra Seat")]
            CBBG = 0,
            [Description("Extra Seat")]
            CBBGTWO = 1,
            [Description("Extra Seat")]
            CBBGTHREE = 2,
            [Description("Extra Seat")]
            CBBGFOUR = 3,
            [Description("Extra Seat")]
            CBBGFIVE = 4,
            [Description("Extra Seat")]
            CBBGSIX = 5,
            [Description("Extra Seat")]
            CBBGSEVEN = 6
        }

    }
}
