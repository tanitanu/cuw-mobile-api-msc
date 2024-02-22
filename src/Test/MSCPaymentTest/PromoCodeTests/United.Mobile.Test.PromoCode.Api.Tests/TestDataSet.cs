using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using United.Definition;
using United.Definition.FormofPayment;
using United.Definition.Shopping;
using United.Persist.Definition.Shopping;
using United.Services.FlightShopping.Common.DisplayCart;
using United.Services.FlightShopping.Common.FlightReservation;

namespace United.Mobile.Test.PromoCode.Api.Tests
{
    public class TestDataSet
    {
        public Object[] Set1()
        {
            var MOBApplyPromoCodeRequestjson = TestDataGenerator.GetFileContent(@"ApplyPromoCode\MOBApplyPromoCodeRequest.json");
            var mOBApplyPromoCodeRequest = JsonConvert.DeserializeObject<List<MOBApplyPromoCodeRequest>>(MOBApplyPromoCodeRequestjson);

            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"ApplyPromoCode\MOBShoppingCart.json");
            var mOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);

            var MOBCSLContentMessagesResponsejson = TestDataGenerator.GetFileContent(@"ApplyPromoCode\MOBCSLContentMessagesResponse.json");
            var mOBCSLContentMessagesResponse = JsonConvert.DeserializeObject<List<MOBCSLContentMessagesResponse>>(MOBCSLContentMessagesResponsejson);

            var LoadReservationAndDisplayCartResponsejson = TestDataGenerator.GetFileContent(@"ApplyPromoCode\LoadReservationAndDisplayCartResponse.json");
            var loadReservationAndDisplayCartResponse = JsonConvert.DeserializeObject<List<LoadReservationAndDisplayCartResponse>>(LoadReservationAndDisplayCartResponsejson);

            var Reservationjson = TestDataGenerator.GetFileContent(@"ApplyPromoCode\Reservation.json");
            var Reservation = JsonConvert.DeserializeObject<List<Reservation>>(Reservationjson);

            var ProfileResponsejson = TestDataGenerator.GetFileContent(@"ApplyPromoCode\ProfileResponse.json");
            var ProfileResponse = JsonConvert.DeserializeObject<List<ProfileResponse>>(ProfileResponsejson);


            var FlightReservationResponsejson = TestDataGenerator.GetFileContent(@"ApplyPromoCode\FlightReservationResponse.json");
            var flightReservationResponse = JsonConvert.DeserializeObject<List<FlightReservationResponse>>(FlightReservationResponsejson);

            var MOBFOPTravelerCertificateResponsejson = TestDataGenerator.GetFileContent(@"ApplyPromoCode\MOBFOPTravelerCertificateResponse.json");
            var MOBFOPTravelerCertificateResponse = JsonConvert.DeserializeObject<List<MOBFOPTravelerCertificateResponse>>(MOBFOPTravelerCertificateResponsejson);

            var sessionjson = TestDataGenerator.GetFileContent(@"ApplyPromoCode\Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);

            var MOBFOPResponsejson = TestDataGenerator.GetFileContent(@"ApplyPromoCode\MOBFOPResponse.json");
            var MOBFOPResponse = JsonConvert.DeserializeObject<List<MOBFOPResponse>>(MOBFOPResponsejson);

            //var CultureInfojson = TestDataGenerator.GetFileContent(@"ApplyPromoCode\CultureInfo.json");
            //var CultureInfo = JsonConvert.DeserializeObject<List<CultureInfo>>(CultureInfojson);

            var mOBApplyPromoCodeResponseJson = TestDataGenerator.GetFileContent(@"ApplyPromoCode\MOBApplyPromoCodeResponse.json");
            var mOBApplyPromoCodeResponse = JsonConvert.DeserializeObject<MOBApplyPromoCodeResponse>(mOBApplyPromoCodeResponseJson);

            var formofPaymentOptionsJson = TestDataGenerator.GetFileContent(@"ApplyPromoCode\FormofPaymentOption.json");
            var formofPaymentOptions = JsonConvert.DeserializeObject<List<List<FormofPaymentOption>>>(formofPaymentOptionsJson);

            return new object[] { mOBApplyPromoCodeRequest[0], mOBShoppingCart[0], mOBCSLContentMessagesResponse[0], loadReservationAndDisplayCartResponse[0], Reservation[0], ProfileResponse[0], flightReservationResponse[0], MOBFOPTravelerCertificateResponse[0], session[0], MOBFOPResponse[0], mOBApplyPromoCodeResponse, formofPaymentOptions[0] };
        }

        public Object[] set1()
        {
            var MOBApplyPromoCodeRequestjson = TestDataGenerator.GetFileContent(@"ApplyPromoCode\MOBApplyPromoCodeRequest.json");
            var mOBApplyPromoCodeRequest = JsonConvert.DeserializeObject<List<MOBApplyPromoCodeRequest>>(MOBApplyPromoCodeRequestjson);

            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"ApplyPromoCode\MOBShoppingCart.json");
            var mOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);

            var MOBCSLContentMessagesResponsejson = TestDataGenerator.GetFileContent(@"ApplyPromoCode\MOBCSLContentMessagesResponse.json");
            var mOBCSLContentMessagesResponse = JsonConvert.DeserializeObject<List<MOBCSLContentMessagesResponse>>(MOBCSLContentMessagesResponsejson);

            var LoadReservationAndDisplayCartResponsejson = TestDataGenerator.GetFileContent(@"ApplyPromoCode\LoadReservationAndDisplayCartResponse.json");
            var loadReservationAndDisplayCartResponse = JsonConvert.DeserializeObject<List<LoadReservationAndDisplayCartResponse>>(LoadReservationAndDisplayCartResponsejson);

            var Reservationjson = TestDataGenerator.GetFileContent(@"ApplyPromoCode\Reservation.json");
            var Reservation = JsonConvert.DeserializeObject<List<Reservation>>(Reservationjson);

            var ProfileResponsejson = TestDataGenerator.GetFileContent(@"ApplyPromoCode\ProfileResponse.json");
            var ProfileResponse = JsonConvert.DeserializeObject<List<ProfileResponse>>(ProfileResponsejson);


            var FlightReservationResponsejson = TestDataGenerator.GetFileContent(@"ApplyPromoCode\FlightReservationResponse.json");
            var flightReservationResponse = JsonConvert.DeserializeObject<List<FlightReservationResponse>>(FlightReservationResponsejson);

            var MOBFOPTravelerCertificateResponsejson = TestDataGenerator.GetFileContent(@"ApplyPromoCode\MOBFOPTravelerCertificateResponse.json");
            var MOBFOPTravelerCertificateResponse = JsonConvert.DeserializeObject<List<MOBFOPTravelerCertificateResponse>>(MOBFOPTravelerCertificateResponsejson);

            var sessionjson = TestDataGenerator.GetFileContent(@"ApplyPromoCode\Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);

            var MOBFOPResponsejson = TestDataGenerator.GetFileContent(@"ApplyPromoCode\MOBFOPResponse.json");
            var MOBFOPResponse = JsonConvert.DeserializeObject<List<MOBFOPResponse>>(MOBFOPResponsejson);

            //var CultureInfojson = TestDataGenerator.GetFileContent(@"ApplyPromoCode\CultureInfo.json");
            //var CultureInfo = JsonConvert.DeserializeObject<List<CultureInfo>>(CultureInfojson);

            return new object[] { mOBApplyPromoCodeRequest[1], mOBShoppingCart[0], mOBCSLContentMessagesResponse[0], loadReservationAndDisplayCartResponse[0], Reservation[0], ProfileResponse[0], flightReservationResponse[0], MOBFOPTravelerCertificateResponse[0], session[0], MOBFOPResponse[0] };
        }


        public Object[] Set2()
        {
            var MOBApplyPromoCodeRequestjson = TestDataGenerator.GetFileContent(@"GetTermsandConditionsByPromoCode\MOBApplyPromoCodeRequest.json");
            var mOBApplyPromoCodeRequest = JsonConvert.DeserializeObject<List<MOBApplyPromoCodeRequest>>(MOBApplyPromoCodeRequestjson);

            var sessionjson = TestDataGenerator.GetFileContent(@"GetTermsandConditionsByPromoCode\Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);



            return new object[] { mOBApplyPromoCodeRequest[0], session[0] };

        }

        public Object[] Set3()
        {
            var MOBApplyPromoCodeRequestjson = TestDataGenerator.GetFileContent(@"RemovePromoCode\MOBApplyPromoCodeRequest.json");
            var mOBApplyPromoCodeRequest = JsonConvert.DeserializeObject<List<MOBApplyPromoCodeRequest>>(MOBApplyPromoCodeRequestjson);

            var sessionjson = TestDataGenerator.GetFileContent(@"RemovePromoCode\Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);


            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"RemovePromoCode\MOBShoppingCart.json");
            var mOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);

            var MOBApplyPromoCodeResponsejson = TestDataGenerator.GetFileContent(@"RemovePromoCode\MOBApplyPromoCodeResponse.json");
            var MOBApplyPromoCodeResponse = JsonConvert.DeserializeObject<List<MOBApplyPromoCodeResponse>>(MOBApplyPromoCodeResponsejson);


            var Reservationjson = TestDataGenerator.GetFileContent(@"RemovePromoCode\Reservation.json");
            var Reservation = JsonConvert.DeserializeObject<List<Reservation>>(Reservationjson);

            var FlightReservationResponsejson = TestDataGenerator.GetFileContent(@"RemovePromoCode\FlightReservationResponse.json");
            var flightReservationResponse = JsonConvert.DeserializeObject<List<FlightReservationResponse>>(FlightReservationResponsejson);

            var formofPaymentOptionsJson = TestDataGenerator.GetFileContent(@"RemovePromoCode\FormofPaymentOption.json");
            var formofPaymentOptions = JsonConvert.DeserializeObject<List<List<FormofPaymentOption>>>(formofPaymentOptionsJson);

            return new object[] { mOBApplyPromoCodeRequest[0], session[0],
                mOBShoppingCart[0], MOBApplyPromoCodeResponse[0],Reservation[0], flightReservationResponse[0] , formofPaymentOptions[0] };
        }

        public Object[] Set5()
        {
            var MOBApplyPromoCodeRequestjson = TestDataGenerator.GetFileContent(@"GetTermsandConditionsByPromoCode\MOBApplyPromoCodeRequest.json");
            var mOBApplyPromoCodeRequest = JsonConvert.DeserializeObject<List<MOBApplyPromoCodeRequest>>(MOBApplyPromoCodeRequestjson);

            var sessionjson = TestDataGenerator.GetFileContent(@"GetTermsandConditionsByPromoCode\Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);



            return new object[] { mOBApplyPromoCodeRequest[0], session[0] };

        }
    }
}


