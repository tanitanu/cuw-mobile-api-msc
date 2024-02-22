using System;
using System.Collections.Generic;

namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBSHOPReshopTrip
    {
        private MOBSHOPTrip originalTrip;
        private bool isReshopTrip = false;
        private MOBSHOPTrip changeTrip;
        private string changeTripTitle;

        public MOBSHOPTrip OriginalTrip
        {
            get { return originalTrip; }
            set { originalTrip = value; }
        }

        public bool IsReshopTrip
        {
            get { return isReshopTrip; }
            set { isReshopTrip = value; }
        }
        public string ChangeTripTitle
        {
            get
            {
                return this.changeTripTitle;
            }
            set
            {
                this.changeTripTitle = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBSHOPTrip ChangeTrip
        {
            get { return changeTrip; }
            set { changeTrip = value; }
        }

        private bool isUsed = false;

        public bool IsUsed
        {
            get { return isUsed; }
            set { isUsed = value; }
        }

        private int originalUsedIndex;

        public int OriginalUsedIndex
        {
            get { return originalUsedIndex; }
            set { originalUsedIndex = value; }
        }
    }
}
