using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBPaxFlight
    {
        private string fltNbr = string.Empty;
        private string fltLegLclOrigDt = string.Empty;
        private MOBAirport arrArpt;
        private string carrIataCd = string.Empty;
        private MOBAirport depArpt;
        private string fltLegLclDepDt = string.Empty;
        private string fltLegSqnr = string.Empty;
        private string lclOutOfBlkDtTm = string.Empty;
        private string lclInBlkDtTm = string.Empty;
        private string arrRteTypeCd = string.Empty;
        private string seatNbr = string.Empty;
        private string psgrStatCd = string.Empty;

        public string PaxStatusCode
        {
            get
            {
                return this.psgrStatCd;
            }
            set
            {
                this.psgrStatCd = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string SeatNumber
        {
            get
            {
                return this.seatNbr;
            }
            set
            {
                this.seatNbr = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string ArrRteTypeCd 
        {
            get
            {
                return this.arrRteTypeCd;
            }
            set
            {
                this.arrRteTypeCd = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FlightNumber
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

        public string FltLegLclOrigDt
        {
            get
            {
                return this.fltLegLclOrigDt;
            }
            set
            {
                this.fltLegLclOrigDt = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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

        public string CarrierCode
        {
            get
            {
                return this.carrIataCd;
            }
            set
            {
                this.carrIataCd = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FltLegLclDepDt
        {
            get
            {
                return this.fltLegLclDepDt;
            }
            set
            {
                this.fltLegLclDepDt = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
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
        public string LclOutOfBlkDtTm
        {
            get
            {
                return this.lclOutOfBlkDtTm;
            }
            set
            {
                this.lclOutOfBlkDtTm = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
        public string LclInBlkDtTm
        {
            get
            {
                return this.lclInBlkDtTm;
            }
            set
            {
                this.lclInBlkDtTm = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }
}
