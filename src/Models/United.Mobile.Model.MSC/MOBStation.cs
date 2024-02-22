using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition
{
    [Serializable()]
    public class MOBStation
    {
        private string code = string.Empty;
        private string sName = string.Empty;
        private string mName = string.Empty;
        private string aFlag = string.Empty;
        private string cityCode = string.Empty;
        private int allAirportFlag;


        public string Code
        {
            get
            {
                return this.code;
            }
            set
            {
                this.code = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public string SName
        {
            get
            {
                return this.sName;
            }
            set
            {
                this.sName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string MName
        {
            get
            {
                return this.mName;
            }
            set
            {
                this.mName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string AFlag
        {
            get
            {
                return this.aFlag;
            }
            set
            {
                this.aFlag = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string CityCode
        {
            get
            {
                return this.cityCode;
            }
            set
            {
                this.cityCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public int AllAirportFlag
        {
            get
            {
                return this.allAirportFlag;
            }
            set
            {
                this.allAirportFlag = value;
            }
        }
    }
}
