using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition.Shopping
{
    [Serializable()]
    public class MOBSHOPOrganizeResultsResponse : MOBResponse
    {
        private MOBSHOPOrganizeResultsReqeust organizeResultsRequest;
        private MOBSHOPAvailability availability;
        private List<string> disclaimer;
        private string cartId = string.Empty;

        public MOBSHOPOrganizeResultsReqeust OrganizeResultsRequest
        {
            get
            {
                return this.organizeResultsRequest;
            }
            set
            {
                this.organizeResultsRequest = value;
            }
        }

        public MOBSHOPAvailability Availability
        {
            get
            {
                return this.availability;
            }
            set
            {
                this.availability = value;
            }
        }

        public List<string> Disclaimer
        {
            get
            {
                return this.disclaimer;
            }
            set
            {
                this.disclaimer = value;
            }
        }

        public string CartId
        {
            get { return cartId; }
            set { cartId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

    }
}
