using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBBundlesMerchandizingResponse : MOBResponse
    {
        private MOBBundlesMerchangdizingRequest request;
        private MOBBundleInfo bundleInfo;

        public MOBBundlesMerchangdizingRequest Request
        {
            get
            {
                return this.request;
            }
            set
            {
                this.request = value;
            }
        }
        public MOBBundleInfo BundleInfo
        {
            get
            {
                return this.bundleInfo;
            }
            set
            {
                this.bundleInfo = value;
            }
        }
    }
}
