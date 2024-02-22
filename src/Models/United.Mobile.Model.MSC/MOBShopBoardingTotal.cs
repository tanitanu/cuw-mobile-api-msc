using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace United.Definition
{
    [Serializable()]
    public class MOBShopBoardingTotal
    {
        private int authorized;
        private int booked;
        private int capacity;
        private int groupBookings;
        private int held;
        private int jumpSeat;
        private int positiveSpace;
        private int reserved;
        private int revenueStandby;
        private int spaceAvailable;
        private int waitList;

        public int Authorized
        {
            get
            {
                return this.authorized;
            }
            set
            {
                this.authorized = value;
            }
        }

        public int Booked
        {
            get
            {
                return this.booked;
            }
            set
            {
                this.booked = value;
            }
        }

        public int Capacity
        {
            get
            {
                return this.capacity;
            }
            set
            {
                this.capacity = value;
            }
        }

        public int GroupBookings
        {
            get
            {
                return this.groupBookings;
            }
            set
            {
                this.groupBookings = value;
            }
        }

        public int Held
        {
            get
            {
                return this.held;
            }
            set
            {
                this.held = value;
            }
        }

        public int JumpSeat
        {
            get
            {
                return this.jumpSeat;
            }
            set
            {
                this.jumpSeat = value;
            }
        }

        public int PositiveSpace
        {
            get
            {
                return this.positiveSpace;
            }
            set
            {
                this.positiveSpace = value;
            }
        }

        public int Reserved
        {
            get
            {
                return this.reserved;
            }
            set
            {
                this.reserved = value;
            }
        }

        public int RevenueStandby
        {
            get
            {
                return this.revenueStandby;
            }
            set
            {
                this.revenueStandby = value;
            }
        }

        public int SpaceAvailable
        {
            get
            {
                return this.spaceAvailable;
            }
            set
            {
                this.spaceAvailable = value;
            }
        }

        public int WaitList
        {
            get
            {
                return this.WaitList;
            }
            set
            {
                this.WaitList = value;
            }
        }
    }
}
