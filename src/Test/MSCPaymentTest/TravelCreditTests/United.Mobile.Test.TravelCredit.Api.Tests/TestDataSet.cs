using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using United.Definition;
using United.Definition.FormofPayment;
using United.Definition.Shopping;
using United.Persist.Definition.FOP;
using United.Persist.Definition.SeatChange;
using United.Persist.Definition.Shopping;
using United.Service.Presentation.SecurityResponseModel;
using United.Services.FlightShopping.Common.DisplayCart;

namespace United.Mobile.Test.TravelCredit.Api.Tests
{
    public class TestDataSet
    {
        public Object[] Set1()
        {
            var MOBFutureFlightCreditRequestjson = TestDataGenerator.GetFileContent(@"FutureFlightCredit\MOBFutureFlightCreditRequest.json");
            var mOBFutureFlightCreditRequest = JsonConvert.DeserializeObject<List<MOBFutureFlightCreditRequest>>(MOBFutureFlightCreditRequestjson);

            var Sessionjson = TestDataGenerator.GetFileContent(@"FutureFlightCredit\Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(Sessionjson);

            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"FutureFlightCredit\MOBShoppingCart.json");
            var mOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);

            var ProfileResponsejson = TestDataGenerator.GetFileContent(@"FutureFlightCredit\ProfileResponse.json");
            var ProfileResponse = JsonConvert.DeserializeObject<List<ProfileResponse>>(ProfileResponsejson);

            var Reservationjson = TestDataGenerator.GetFileContent(@"FutureFlightCredit\Reservation.json");
            var Reservation = JsonConvert.DeserializeObject<List<Reservation>>(Reservationjson);

            var MOBCSLContentMessagesResponsejson = TestDataGenerator.GetFileContent(@"FutureFlightCredit\MOBCSLContentMessagesResponse.json");
            var mOBCSLContentMessagesResponse = JsonConvert.DeserializeObject<List<MOBCSLContentMessagesResponse>>(MOBCSLContentMessagesResponsejson);

            var LoadReservationAndDisplayCartResponsejson = TestDataGenerator.GetFileContent(@"FutureFlightCredit\LoadReservationAndDisplayCartResponse.json");
            var loadReservationAndDisplayCartResponse = JsonConvert.DeserializeObject<List<LoadReservationAndDisplayCartResponse>>(LoadReservationAndDisplayCartResponsejson);

            var MOBFutureFlightCreditResponsejson = TestDataGenerator.GetFileContent(@"FutureFlightCredit\MOBFutureFlightCreditResponse.json");
            var MOBFutureFlightCreditResponse = JsonConvert.DeserializeObject<List<MOBFutureFlightCreditResponse>>(MOBFutureFlightCreditResponsejson);

            var PKDispenserKeyjson = TestDataGenerator.GetFileContent(@"FutureFlightCredit\PKDispenserKey.json");
            var pKDispenserKey = JsonConvert.DeserializeObject<List<PKDispenserKey>>(PKDispenserKeyjson);


            var eligibleFormofPaymentsjson = TestDataGenerator.GetFileContent(@"FutureFlightCredit\EligibleFormofPayments.json");
            var eligibleFormofPayments = JsonConvert.DeserializeObject<List<List<FormofPaymentOption>>>(eligibleFormofPaymentsjson);

            return new object[] { mOBFutureFlightCreditRequest[0], session[0], mOBShoppingCart[0], ProfileResponse[0], Reservation[0], mOBCSLContentMessagesResponse[0], loadReservationAndDisplayCartResponse[0], MOBFutureFlightCreditResponse[0], pKDispenserKey[0] , eligibleFormofPayments [0]};
        }
        public Object[] Set2()
        {
            var MOBFOPFutureFlightCreditRequestjson = TestDataGenerator.GetFileContent(@"RemoveFutureFlightCredit\MOBFOPFutureFlightCreditRequest.json");
            var MOBFOPFutureFlightCreditRequest = JsonConvert.DeserializeObject<List<MOBFOPFutureFlightCreditRequest>>(MOBFOPFutureFlightCreditRequestjson);

            var Sessionjson = TestDataGenerator.GetFileContent(@"RemoveFutureFlightCredit\Session.json");
            var Session = JsonConvert.DeserializeObject<List<Session>>(Sessionjson);

            var MOBFutureFlightCreditResponsejson = TestDataGenerator.GetFileContent(@"RemoveFutureFlightCredit\MOBFutureFlightCreditResponse.json");
            var MOBFutureFlightCreditResponse = JsonConvert.DeserializeObject<List<MOBFutureFlightCreditResponse>>(MOBFutureFlightCreditResponsejson);

            var ProfileResponsejson = TestDataGenerator.GetFileContent(@"RemoveFutureFlightCredit\ProfileResponse.json");
            var ProfileResponse = JsonConvert.DeserializeObject<List<ProfileResponse>>(ProfileResponsejson);



            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"RemoveFutureFlightCredit\MOBShoppingCart.json");
            var MOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);

            var Reservationjson = TestDataGenerator.GetFileContent(@"RemoveFutureFlightCredit\Reservation.json");
            var Reservation = JsonConvert.DeserializeObject<List<Reservation>>(Reservationjson);

            var MOBCSLContentMessagesResponsejson = TestDataGenerator.GetFileContent(@"RemoveFutureFlightCredit\MOBCSLContentMessagesResponse.json");
            var MOBCSLContentMessagesResponse = JsonConvert.DeserializeObject<List<MOBCSLContentMessagesResponse>>(MOBCSLContentMessagesResponsejson);

            var SeatChangeStatejson = TestDataGenerator.GetFileContent(@"RemoveFutureFlightCredit\SeatChangeState.json");
            var seatChangeState = JsonConvert.DeserializeObject<List<SeatChangeState>>(SeatChangeStatejson);

            return new object[] { MOBFOPFutureFlightCreditRequest[0], Session[0], MOBFutureFlightCreditResponse[0], ProfileResponse[0], MOBShoppingCart[0], Reservation[0], MOBCSLContentMessagesResponse[0], seatChangeState[0] };
        }
        public Object[] Set3()
        {
            var MOBFOPLookUpTravelCreditRequestjson = TestDataGenerator.GetFileContent(@"LookUpTravelCredit\MOBFOPLookUpTravelCreditRequest.json");
            var MOBFOPLookUpTravelCreditRequest = JsonConvert.DeserializeObject<List<MOBFOPLookUpTravelCreditRequest>>(MOBFOPLookUpTravelCreditRequestjson);

            var Sessionjson = TestDataGenerator.GetFileContent(@"LookUpTravelCredit\Session.json");
            var Session = JsonConvert.DeserializeObject<List<Session>>(Sessionjson);

            var MOBFOPResponsejson = TestDataGenerator.GetFileContent(@"LookUpTravelCredit\MOBFOPResponse.json");
            var MOBFOPResponse = JsonConvert.DeserializeObject<List<MOBFOPResponse>>(MOBFOPResponsejson);

            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"LookUpTravelCredit\MOBShoppingCart.json");
            var MOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);

            var Reservationjson = TestDataGenerator.GetFileContent(@"LookUpTravelCredit\Reservation.json");
            var Reservation = JsonConvert.DeserializeObject<List<Reservation>>(Reservationjson);

            var ProfileResponsejson = TestDataGenerator.GetFileContent(@"LookUpTravelCredit\ProfileResponse.json");
            var ProfileResponse = JsonConvert.DeserializeObject<List<ProfileResponse>>(ProfileResponsejson);

            var ProfileFOPCreditCardResponsejson = TestDataGenerator.GetFileContent(@"LookUpTravelCredit\ProfileFOPCreditCardResponse.json");
            var profileFOPCreditCardResponse = JsonConvert.DeserializeObject<List<ProfileFOPCreditCardResponse>>(ProfileFOPCreditCardResponsejson);

            var MOBCSLContentMessagesResponsejson = TestDataGenerator.GetFileContent(@"LookUpTravelCredit\MOBCSLContentMessagesResponse.json");
            var MOBCSLContentMessagesResponse = JsonConvert.DeserializeObject<List<MOBCSLContentMessagesResponse>>(MOBCSLContentMessagesResponsejson);

            var eligibleFormofPaymentsjson = TestDataGenerator.GetFileContent(@"LookUpTravelCredit\EligibleFormofPayments.json");
            var eligibleFormofPayments = JsonConvert.DeserializeObject<List<List<FormofPaymentOption>>>(eligibleFormofPaymentsjson);


            return new object[] { MOBFOPLookUpTravelCreditRequest[0], Session[0], MOBFOPResponse[0], MOBShoppingCart[0], Reservation[0], ProfileResponse[0], profileFOPCreditCardResponse[0], MOBCSLContentMessagesResponse[0], eligibleFormofPayments[0] };
        }
        public Object[] Set4()
        {
            var MOBFOPManageTravelCreditRequestjson = TestDataGenerator.GetFileContent(@"ManageTravelCredit\MOBFOPManageTravelCreditRequest.json");
            var MOBFOPManageTravelCreditRequest = JsonConvert.DeserializeObject<List<MOBFOPManageTravelCreditRequest>>(MOBFOPManageTravelCreditRequestjson);

            var Sessionjson = TestDataGenerator.GetFileContent(@"ManageTravelCredit\Session.json");
            var Session = JsonConvert.DeserializeObject<List<Session>>(Sessionjson);



            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"ManageTravelCredit\MOBShoppingCart.json");
            var MOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);

            var Reservationjson = TestDataGenerator.GetFileContent(@"ManageTravelCredit\Reservation.json");
            var Reservation = JsonConvert.DeserializeObject<List<Reservation>>(Reservationjson);

            var MOBFOPResponsejson = TestDataGenerator.GetFileContent(@"ManageTravelCredit\MOBFOPResponse.json");
            var MOBFOPResponse = JsonConvert.DeserializeObject<List<MOBFOPResponse>>(MOBFOPResponsejson);

            var ProfileResponsejson = TestDataGenerator.GetFileContent(@"ManageTravelCredit\ProfileResponse.json");
            var ProfileResponse = JsonConvert.DeserializeObject<List<ProfileResponse>>(ProfileResponsejson);


            var ProfileFOPCreditCardResponsejson = TestDataGenerator.GetFileContent(@"ManageTravelCredit\ProfileFOPCreditCardResponse.json");
            var profileFOPCreditCardResponse = JsonConvert.DeserializeObject<List<ProfileFOPCreditCardResponse>>(ProfileFOPCreditCardResponsejson);




            var MOBCSLContentMessagesResponsejson = TestDataGenerator.GetFileContent(@"ManageTravelCredit\MOBCSLContentMessagesResponse.json");
            var MOBCSLContentMessagesResponse = JsonConvert.DeserializeObject<List<MOBCSLContentMessagesResponse>>(MOBCSLContentMessagesResponsejson);

            return new object[] { MOBFOPManageTravelCreditRequest[0], Session[0], MOBShoppingCart[0], Reservation[0], MOBFOPResponse[0], ProfileResponse[0], profileFOPCreditCardResponse[0], MOBCSLContentMessagesResponse[0] };
        }
    }
}

