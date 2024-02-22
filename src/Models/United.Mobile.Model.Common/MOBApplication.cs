using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Mobile.Model.Common
{
    [Serializable]
    public class MOBApplication
    {
        private int id;
        private string name = string.Empty;
        private MOBVersion version;
        private bool isProduction;

        public int Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public MOBVersion Version
        {
            get
            {
                return this.version;
            }
            set
            {
                this.version = value;
            }
        }

        public bool IsProduction
        {
            get
            {
                return this.isProduction;
            }
            set
            {
                this.isProduction = value;
            }
        }
    }
}
