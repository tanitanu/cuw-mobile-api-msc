using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    public class MOBSpatialLocationResponse : MOBResponse
    {
        private bool succeed;

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
