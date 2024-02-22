using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBBagTagCheckInDetail
    {
        private string airlineCd = string.Empty;
        private string fltNbr = string.Empty;
        private MOBAirport arrArpt;
        private MOBAirport depArpt;
        private string fltLegSqnr = string.Empty;
        private MOBBagStatus bagStatus;

        public string AirlineCode
        {
            get
            {
                return this.airlineCd;
            }
            set
            {
                this.airlineCd = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string flightNumber
        {
            get
            {
                return this.fltNbr;
            }
            set
            {
                this.fltNbr = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FltLegSqnr
        {
            get
            {
                return this.fltLegSqnr;
            }
            set
            {
                this.fltLegSqnr = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBAirport ArrAirport
        {
            get
            {
                return this.arrArpt;
            }
            set
            {
                this.arrArpt = value;
            }
        }

        public MOBAirport DepAirport
        {
            get
            {
                return this.depArpt;
            }
            set
            {
                this.depArpt = value;
            }
        }

        public MOBBagStatus BagStatus
        {
            get
            {
                return this.bagStatus;
            }
            set
            {
                this.bagStatus = value;
            }
        }
    }
}
