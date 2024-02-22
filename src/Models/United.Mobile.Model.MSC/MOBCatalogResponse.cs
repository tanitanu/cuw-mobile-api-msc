using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBCatalogResponse : MOBResponse
    {
        public MOBCatalogResponse()
            : base()
        {
        }

        private List<MOBItem> items;
        private bool succeed = false;

        public List<MOBItem> Items
        {
            get
            {
                return this.items;
            }
            set
            {
                this.items = value;
            }
        }

        public bool Succeed
        {
            get
            {
                return this.succeed;
            }
            set
            {
                this.succeed = value;
            }
        }
    }
}
