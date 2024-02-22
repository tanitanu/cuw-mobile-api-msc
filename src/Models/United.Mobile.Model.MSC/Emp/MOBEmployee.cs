using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.Emp
{
    class MOBEmployee
    {
        

       
        private string eid;

       
        private string l1;

       
        private string l2;

       
        private string fn;

       
        private string mn;

       
        private string ln;

       
        private string ns;

       
        private string bd;

       
        private string g;

       
        private string s;

       
        private string cn;

       
        private string cr;

       
        private string cd;

       
        private string ccs;

       
        private string cca;

       
        private string cc;

       
        private string mc;

       
        private string erid;

       
        private string ed;

       
        private bool oti;

       
        private bool pl;

       
        private bool atc;

       
        private string tr;

       
        private string cm;

       
        private bool vp;

       
        private string ccd;

       
        private string gid;

       
        //private List<SSRInfo> ssrs;

       
        private MOBEmpTCDInfo empTCDInfo;

       
        private string aei = "";

       
        private string stui = "";

       
        private string pstai = "";

       
        private string emtai = "";

        public string EmployeeID
        {
            get { return this.eid; }
            set { this.eid = value; }
        }

        public string Level1
        {
            get { return this.l1; }
            set { this.l1 = value; }
        }

        public string Level2
        {
            get { return this.l2; }
            set { this.l2 = value; }
        }

        public string FirstName
        {
            get { return this.fn; }
            set { this.fn = value; }
        }

        public string MiddleName
        {
            get { return this.mn; }
            set { this.mn = value; }
        }

        public string LastName
        {
            get { return this.ln; }
            set { this.ln = value; }
        }

        public string NameSuffix
        {
            get { return this.ns; }
            set { this.ns = value; }
        }

        public string BirthDate
        {
            get { return this.bd; }
            set { this.bd = value; }
        }

        public string Gender
        {
            get { return this.g; }
            set { this.g = value; }
        }

        public string Status
        {
            get { return this.s; }
            set { this.s = value; }
        }

        public string CommuteOnly
        {
            get { return this.cn; }
            set { this.cn = value; }
        }

        public string CommuteOrigin
        {
            get { return this.cr; }
            set { this.cr = value; }
        }

        public string CommuteDestination
        {
            get { return this.cd; }
            set { this.cd = value; }
        }

        public string CheckCashSW
        {
            get { return this.ccs; }
            set { this.ccs = value; }
        }

        public string CheckCashAuth
        {
            get { return this.cca; }
            set { this.cca = value; }
        }

        public string CityCode
        {
            get { return this.cc; }
            set { this.cc = value; }
        }

        public string MailCode
        {
            get { return this.mc; }
            set { this.mc = value; }
        }

        public string EmployerID
        {
            get { return this.eid; }
            set { this.eid = value; }
        }

        public string EmployerDescription
        {
            get { return this.ed; }
            set { this.ed = value; }
        }

        public bool OATravelIndicator
        {
            get { return this.oti; }
            set { this.oti = value; }
        }

        public bool PersonalLimited
        {
            get { return this.pl; }
            set { this.pl = value; }
        }

        public bool AnnualTravelCard
        {
            get { return this.atc; }
            set { this.atc = value; }
        }

        public string TravelRegion
        {
            get { return this.tr; }
            set { this.tr = value; }
        }

        public string Comments
        {
            get { return this.cm; }
            set { this.cm = value; }
        }

        public bool ViewPBTs
        {
            get { return this.vp; }
            set { this.vp = value; }
        }

        public string CarrierCode
        {
            get { return this.ccd; }
            set { this.ccd = value; }
        }

        public string GlobalID
        {
            get { return this.gid; }
            set { this.gid = value; }
        }

        //public List<SSRInfo> SSRs
        //{
        //    get { return this.ssrs; }
        //    set { this.ssrs = value; }
        //}

        public MOBEmpTCDInfo EmpTCDInfo
        {
            get { return this.empTCDInfo; }
            set { this.empTCDInfo = value; }
        }

        public string AltEmpId
        {
            get { return aei; }
            set { aei = value; }
        }

        public string SeflTicketAuthInd
        {
            get { return stui; }
            set { stui = value; }
        }

        public string PSTicketAuthInd
        {
            get { return pstai; }
            set { pstai = value; }
        }

        public string EmerTicketAuthInd
        {
            get { return emtai; }
            set { emtai = value; }
        }
    }
    
}
