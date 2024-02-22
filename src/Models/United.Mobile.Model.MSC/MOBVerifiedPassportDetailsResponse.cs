using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Mobile.Model.Common;
namespace United.Definition
{
    [Serializable()]
    public class MOBVerifiedPassportDetailsResponse : MOBResponse
    {
        public MOBVerifiedPassportDetailsResponse()
            : base()
        {
        }

        private bool isPassportVerified;

        public bool IsPassportVerified
        {
            get { return this.isPassportVerified; }
            set
            {
                this.isPassportVerified = value;
            }
        }

        private int insertedRowID;
        public int InsertedRowID
        {
            get { return this.insertedRowID; }
            set
            {
                this.insertedRowID = value;
            }
        }

        private MOBVerifiedPassportDetails verifiedPassportDetails;

        public MOBVerifiedPassportDetails VerifiedPassportDetails
        {
            get { return this.verifiedPassportDetails; }
            set
            {
                this.verifiedPassportDetails = value;
            }
        }

    }
}
