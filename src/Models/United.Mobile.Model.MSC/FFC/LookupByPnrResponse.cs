using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using United.Service.Presentation.CommonModel;

namespace United.Definition.FFC
{
    [Serializable]
    public sealed class LookupByPnrResponse
    {
        public List<TravelCreditDetail> TravelCreditDetails { get; set; }
        public ServiceResponse Response { get; set; }
        public string Message { get; set; }
        public bool IsSuccessful { get; set; }
    }

    [Serializable]
    public sealed class TravelCreditDetail
    {
         public short PassengerIndex { get; set; }
        public bool HasActiveTCMatch { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
        public string ZipExt { get; set; }
        public string Phone { get; set; }
        public List<TravelCredit> TravelCreditList { get; set; }
        public string DateOfBirth = "01/01/1900";
    }
}
