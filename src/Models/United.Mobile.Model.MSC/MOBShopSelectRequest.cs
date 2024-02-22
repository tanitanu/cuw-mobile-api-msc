using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBShopSelectRequest : MOBRequest
    {
        private string token = string.Empty;
        private string sessionId = string.Empty;
        private string cartId = string.Empty;
        private string bbxSolutionSetId = string.Empty;
        private string bbxCellId = string.Empty;
        private bool awardTravel;

        public string Token
        {
            get
            {
                return this.token;
            }
            set
            {
                this.token = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string SessionId
        {
            get
            {
                return this.sessionId;
            }
            set
            {
                this.sessionId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string CartId
        {
            get
            {
                return this.cartId;
            }
            set
            {
                this.cartId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string BBXSolutionSetId
        {
            get
            {
                return this.bbxSolutionSetId;
            }
            set
            {
                this.bbxSolutionSetId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string BBXCellId
        {
            get
            {
                return this.bbxCellId;
            }
            set
            {
                this.bbxCellId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public bool AwardTravel
        {
            get
            {
                return this.awardTravel;
            }
            set
            {
                this.awardTravel = value;
            }
        }
    }
}
