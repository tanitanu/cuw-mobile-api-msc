using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition;
using United.Definition.Shopping;
using United.Mobile.Model.Common;

namespace United.Persist.Definition.Shopping
{
    [Serializable()]
    public class CSLReservation : IPersist
    {
        #region IPersist Members

        private string objectName = "United.Persist.Definition.Shopping.CSLReservation";
        public string ObjectName
        {
            get
            {
                return this.objectName;
            }
            set
            {
                this.objectName = value;
            }
        }

        #endregion

        public string SessionId {get; set;}

        private MOBResReservation _cslReservation; 
        public MOBResReservation Reservation {
            get {
                return _cslReservation;
            }
            set {
                _cslReservation = value;
                isDirty = true;
            }
        }

        private bool isDirty;
        private MOBSHOPReservation uiReservation;
        public MOBSHOPReservation UIReservation {
            get {
                if (isDirty) {
                    // do the translation
                    uiReservation = new MOBSHOPReservation();
                    //transfer travelers..
                    //commented dhanush
                    //uiReservation.Travelers = transferTravelers(Reservation.Travelers);
                    return uiReservation;
                }
                else {
                    return uiReservation;
                }
            }
        }
        //commented dhanush
        //private List<MOBSHOPTraveler> transferTravelers (List<MOBResTraveler> travelers) {
        //    List<MOBSHOPTraveler> mobTravelers = null;
        //    if(travelers != null) {
        //        United.Persist.Definition.Shopping.FlightConfirmTravelerResponse response = new Shopping.FlightConfirmTravelerResponse();
        //        response = FilePersist.Load<United.Persist.Definition.Shopping.FlightConfirmTravelerResponse>(SessionId, response.ObjectName);
        //        mobTravelers = new List<MOBSHOPTraveler>();
        //        foreach(var traveler in travelers) {
        //            MOBSHOPTraveler mobTraveler = new MOBSHOPTraveler();
        //            if(traveler.Person != null) {
        //                mobTraveler.Person = new MOBSHOPPerson();
        //                //mobTraveler.Person.ChildIndicator = traveler.Person.ChildIndicator;
        //                mobTraveler.CustomerId = long.Parse(traveler.Person.CustomerId);
        //                mobTraveler.Person.DateOfBirth = traveler.Person.DateOfBirth;
        //                mobTraveler.Person.Title = traveler.Person.Title;
        //                mobTraveler.Person.GivenName = traveler.Person.GivenName;
        //                mobTraveler.Person.MiddleName = traveler.Person.MiddleName;
        //                mobTraveler.Person.Surname = traveler.Person.Surname;
        //                mobTraveler.Person.Suffix = traveler.Person.Suffix;
        //                mobTraveler.Person.Suffix = traveler.Person.Sex;
        //                //populate phones
        //                mobTraveler.Person.Phones = populatePhonesForTraveler(mobTraveler, response.Response.Travelers);
        //                if(traveler.Person.Documents != null) {
        //                    mobTraveler.Person.Documents = new List<MOBSHOPDocument>();
        //                    foreach(var dcoument in traveler.Person.Documents) {
        //                        MOBSHOPDocument mobDocument = new MOBSHOPDocument();
        //                        mobDocument.DocumentId = dcoument.DocumentId;
        //                        mobDocument.KnownTravelerNumber = dcoument.KnownTravelerNumber;
        //                        mobDocument.RedressNumber = dcoument.RedressNumber;
        //                        mobTraveler.Person.Documents.Add(mobDocument);
        //                        }
        //                    }
        //                }

        //            if(traveler.LoyaltyProgramProfile != null) {
        //                mobTraveler.LoyaltyProgramProfile = new MOBSHOPLoyaltyProgramProfile();
        //                mobTraveler.LoyaltyProgramProfile.CarrierCode = traveler.LoyaltyProgramProfile.CarrierCode;
        //                mobTraveler.LoyaltyProgramProfile.MemberId = traveler.LoyaltyProgramProfile.MemberId;
        //            }

        //            mobTravelers.Add(mobTraveler);
        //        }
        //    }

        //    return mobTravelers;
        // }

        private List<MOBPhone> populatePhonesForTraveler (MOBSHOPTraveler traveler, List<MOBSHOPTraveler> confirmedTravelers) {
            List<MOBPhone> phones = null;
            foreach (MOBSHOPTraveler confirmedTraveler in confirmedTravelers) {
                if (confirmedTraveler.CustomerId == traveler.CustomerId) {
                    phones = confirmedTraveler.Person.Phones;
                    break;
                }
            }
            return phones;

         }
    }
}
