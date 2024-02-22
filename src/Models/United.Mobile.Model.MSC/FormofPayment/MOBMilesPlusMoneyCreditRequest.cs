using System;

namespace United.Definition.FormofPayment
{
    [Serializable()]
    public class MOBMilesPlusMoneyCreditRequest : MOBShoppingRequest
    {
        private double? miles;
        private string lastName;

        private string sessionId = string.Empty;
        private string email;

        public string Email
        {
            get { return email; }
            set { email = value; }
        }
        public string SessionId
        {
            get
            {
                return this.sessionId;
            }
            set
            {
                this.sessionId = value;
            }
        }

        public double? Miles
        {
            get { return miles; }
            set { miles = value; }
        }



        public string LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }

    }

}
