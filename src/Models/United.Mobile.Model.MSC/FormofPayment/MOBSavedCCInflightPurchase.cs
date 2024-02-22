using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace United.Definition.FormofPayment
{
    [Serializable()]
    public class MOBSavedCCInflightPurchase
    {
        private string cardInformation;
        private string travelers;
        private string flights;
        private string persistentToken;
        public string CardInformation
        {
            get
            {
                return this.cardInformation;
            }
            set
            {
                this.cardInformation = value;
            }
        }
        public string Travelers
        {
            get
            {
                return this.travelers;
            }
            set
            {
                this.travelers = value;
              
            }
        }
        public string Flights
        {
            get
            {
                return this.flights;
            }
            set
            {
                this.flights = value;

            }
        }
        public string PersistentToken
        {
            get
            {
                return this.persistentToken;
            }
            set
            {
                this.persistentToken = value;
            }
        }
    }
}
