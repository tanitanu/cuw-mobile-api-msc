using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using United.Definition;
using United.Definition.Shopping;
using United.Persist.Definition.FOP;
using United.Persist.Definition.SeatChange;
using United.Persist.Definition.Shopping;
using United.Service.Presentation.RefundModel;
using United.Service.Presentation.SecurityResponseModel;

namespace United.Mobile.Test.Payment.Api.Tests
{
    public class TestDataSet
    {
        public Object[] Set1()
        {
            var MOBFOPAcquirePaymentTokenRequestjson = TestDataGenerator.GetFileContent(@"GetPaymentToken\MOBFOPAcquirePaymentTokenRequest.json");
            var mOBFOPAcquirePaymentTokenRequest = JsonConvert.DeserializeObject<List<MOBFOPAcquirePaymentTokenRequest>>(MOBFOPAcquirePaymentTokenRequestjson);



            var Sessionjson = TestDataGenerator.GetFileContent(@"GetPaymentToken\Session.json");
            var Session = JsonConvert.DeserializeObject<List<Session>>(Sessionjson);
            return new object[] { mOBFOPAcquirePaymentTokenRequest[0], Session[0] };
        }

        public Object[] set1_1()
        {
            var MOBFOPAcquirePaymentTokenRequestjson = TestDataGenerator.GetFileContent(@"GetPaymentToken\MOBFOPAcquirePaymentTokenRequest.json");
            var mOBFOPAcquirePaymentTokenRequest = JsonConvert.DeserializeObject<List<MOBFOPAcquirePaymentTokenRequest>>(MOBFOPAcquirePaymentTokenRequestjson);

            var Sessionjson = TestDataGenerator.GetFileContent(@"GetPaymentToken\Session.json");
            var Session = JsonConvert.DeserializeObject<List<Session>>(Sessionjson);

            return new object[] { mOBFOPAcquirePaymentTokenRequest[1], Session[0]};
        }

        public Object[] set1_2()
        {
            var MOBFOPAcquirePaymentTokenRequestjson = TestDataGenerator.GetFileContent(@"GetPaymentToken\MOBFOPAcquirePaymentTokenRequest.json");
            var mOBFOPAcquirePaymentTokenRequest = JsonConvert.DeserializeObject<List<MOBFOPAcquirePaymentTokenRequest>>(MOBFOPAcquirePaymentTokenRequestjson);

            var Sessionjson = TestDataGenerator.GetFileContent(@"GetPaymentToken\Session.json");
            var Session = JsonConvert.DeserializeObject<List<Session>>(Sessionjson);

            return new object[] { mOBFOPAcquirePaymentTokenRequest[2], Session[0] };
        }

        public Object[] Set2()
        {
            var MOBPersistFormofPaymentRequestjson = TestDataGenerator.GetFileContent(@"PersistFormofPaymentDetails\MOBPersistFormofPaymentRequest.json");
            var mOBPersistFormofPaymentRequest = JsonConvert.DeserializeObject<List<MOBPersistFormofPaymentRequest>>(MOBPersistFormofPaymentRequestjson);


            var Sessionjson = TestDataGenerator.GetFileContent(@"PersistFormofPaymentDetails\Session.json");
            var Session = JsonConvert.DeserializeObject<List<Session>>(Sessionjson);

            var Reservationjson = TestDataGenerator.GetFileContent(@"PersistFormofPaymentDetails\Reservation.json");
            var Reservation = JsonConvert.DeserializeObject<List<Reservation>>(Reservationjson);


            var ProfileResponsejson = TestDataGenerator.GetFileContent(@"PersistFormofPaymentDetails\ProfileResponse.json");
            var ProfileResponse = JsonConvert.DeserializeObject<List<ProfileResponse>>(ProfileResponsejson);
            
            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"PersistFormofPaymentDetails\MOBShoppingCart.json");
            var MOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);

            var MOBFOPAcquirePaymentTokenResponsejson = TestDataGenerator.GetFileContent(@"PersistFormofPaymentDetails\MOBFOPAcquirePaymentTokenResponse.json");
            var mOBFOPAcquirePaymentTokenResponse = JsonConvert.DeserializeObject<List<MOBFOPAcquirePaymentTokenResponse>>(MOBFOPAcquirePaymentTokenResponsejson);

            var ProfileFOPCreditCardResponsejson = TestDataGenerator.GetFileContent(@"PersistFormofPaymentDetails\ProfileFOPCreditCardResponse.json");
            var profileFOPCreditCardResponse = JsonConvert.DeserializeObject<List<ProfileFOPCreditCardResponse>>(ProfileFOPCreditCardResponsejson);

            var MOBVormetricKeysjson = TestDataGenerator.GetFileContent(@"PersistFormofPaymentDetails\MOBVormetricKeys.json");
            var mOBVormetricKeys = JsonConvert.DeserializeObject<List<MOBVormetricKeys>>(MOBVormetricKeysjson);

            var MOBCCAdStatementjson = TestDataGenerator.GetFileContent(@"PersistFormofPaymentDetails\MOBCCAdStatement.json");
            var mOBCCAdStatement = JsonConvert.DeserializeObject<List<MOBCCAdStatement>>(MOBCCAdStatementjson);

            var SeatChangeStatejson = TestDataGenerator.GetFileContent(@"PersistFormofPaymentDetails\SeatChangeState.json");
            var seatChangeState = JsonConvert.DeserializeObject<List<SeatChangeState>>(SeatChangeStatejson);



            return new object[] { mOBPersistFormofPaymentRequest[0], Session[0], Reservation[0], ProfileResponse[0], MOBShoppingCart[0], mOBFOPAcquirePaymentTokenResponse[0], profileFOPCreditCardResponse[0], mOBVormetricKeys[0], mOBCCAdStatement[0], seatChangeState[0]};
        }
        public Object[] Set3()
        {
            var MOBPersistFormofPaymentRequestjson = TestDataGenerator.GetFileContent(@"GetCreditCardToken\MOBPersistFormofPaymentRequest.json");
            var mOBPersistFormofPaymentRequest = JsonConvert.DeserializeObject<List<MOBPersistFormofPaymentRequest>>(MOBPersistFormofPaymentRequestjson);


            var Sessionjson = TestDataGenerator.GetFileContent(@"GetCreditCardToken\Session.json");
            var Session = JsonConvert.DeserializeObject<List<Session>>(Sessionjson);

            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"GetCreditCardToken\MOBShoppingCart.json");
            var MOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);

            var Reservationjson = TestDataGenerator.GetFileContent(@"GetCreditCardToken\Reservation.json");
            var Reservation = JsonConvert.DeserializeObject<List<Reservation>>(Reservationjson);

          
            var MOBVormetricKeysjson = TestDataGenerator.GetFileContent(@"GetCreditCardToken\MOBVormetricKeys.json");
            var mOBVormetricKeys = JsonConvert.DeserializeObject<List<MOBVormetricKeys>>(MOBVormetricKeysjson);

            var MOBCCAdStatementjson = TestDataGenerator.GetFileContent(@"GetCreditCardToken\MOBCCAdStatement.json");
            var mOBCCAdStatement = JsonConvert.DeserializeObject<List<MOBCCAdStatement>>(MOBCCAdStatementjson);


            var MOBCSLContentMessagesResponsejson = TestDataGenerator.GetFileContent(@"GetCreditCardToken\MOBCSLContentMessagesResponse.json");
            var mOBCSLContentMessagesResponse = JsonConvert.DeserializeObject<List<MOBCSLContentMessagesResponse>>(MOBCSLContentMessagesResponsejson);

            var formofPaymentOptionsjson = TestDataGenerator.GetFileContent(@"GetCreditCardToken\FormofPaymentOption.json");
            var formofPaymentOptions = JsonConvert.DeserializeObject<List<List<FormofPaymentOption>>>(formofPaymentOptionsjson);

            return new object[] { mOBPersistFormofPaymentRequest[0], Session[0], MOBShoppingCart[0], Reservation[0], mOBVormetricKeys[0], mOBCCAdStatement[0], mOBCSLContentMessagesResponse[0] , formofPaymentOptions[0] };
        }
        public Object[] Set4()
        {

            var MOBSHOPMetaSelectTripRequestjson = TestDataGenerator.GetFileContent(@"GetCartInformation\MOBSHOPMetaSelectTripRequest.json");
            var mOBSHOPMetaSelectTripRequest = JsonConvert.DeserializeObject<List<MOBSHOPMetaSelectTripRequest>>(MOBSHOPMetaSelectTripRequestjson);

            var MOBRegisterOfferResponsejson = TestDataGenerator.GetFileContent(@"GetCartInformation\MOBRegisterOfferResponse.json");
            var mOBRegisterOfferResponse = JsonConvert.DeserializeObject<List<MOBRegisterOfferResponse>>(MOBRegisterOfferResponsejson);


            var CSLShopRequestjson = TestDataGenerator.GetFileContent(@"GetCartInformation\CSLShopRequest.json");
            var cSLShopRequest = JsonConvert.DeserializeObject<List<CSLShopRequest>>(CSLShopRequestjson);

            var PKDispenserKeyjson = TestDataGenerator.GetFileContent(@"GetCartInformation\PKDispenserKey.json");
            var pKDispenserKey = JsonConvert.DeserializeObject<List<PKDispenserKey>>(PKDispenserKeyjson);

            var Sessionjson = TestDataGenerator.GetFileContent(@"GetCartInformation\Session.json");
            var Session = JsonConvert.DeserializeObject<List<Session>>(Sessionjson);

            var mOBShoppingCartjson = TestDataGenerator.GetFileContent(@"GetCartInformation\MOBShoppingCart.json");
            var mOBShoppingCart = JsonConvert.DeserializeObject<MOBShoppingCart>(mOBShoppingCartjson);


            return new object[] { mOBSHOPMetaSelectTripRequest[0], mOBRegisterOfferResponse[0], cSLShopRequest[0], pKDispenserKey[0], Session[0] , mOBShoppingCart };
        }
        public Object[] Set5()
        {
            var MOBFOPTravelerBankRequestjson = TestDataGenerator.GetFileContent(@"TravelBankCredit\MOBFOPTravelerBankRequest.json");
            var mOBFOPTravelerBankRequest = JsonConvert.DeserializeObject<List<MOBFOPTravelerBankRequest>>(MOBFOPTravelerBankRequestjson);

            var Sessionjson = TestDataGenerator.GetFileContent(@"TravelBankCredit\Session.json");
            var Session = JsonConvert.DeserializeObject<List<Session>>(Sessionjson);

            var SeatChangeStatejson = TestDataGenerator.GetFileContent(@"TravelBankCredit\SeatChangeState.json");
            var seatChangeState = JsonConvert.DeserializeObject<List<SeatChangeState>>(SeatChangeStatejson);

            var Reservationjson = TestDataGenerator.GetFileContent(@"TravelBankCredit\Reservation.json");
            var Reservation = JsonConvert.DeserializeObject<List<Reservation>>(Reservationjson);

            var ReservationDetailjson = TestDataGenerator.GetFileContent(@"TravelBankCredit\ReservationDetail.json");
            var reservationDetail = JsonConvert.DeserializeObject<List<ReservationDetail>>(ReservationDetailjson);

            var ProfileResponsejson = TestDataGenerator.GetFileContent(@"TravelBankCredit\ProfileResponse.json");
            var ProfileResponse = JsonConvert.DeserializeObject<List<ProfileResponse>>(ProfileResponsejson);

            var mOBFOPResponsejson = TestDataGenerator.GetFileContent(@"TravelBankCredit\MOBFOPResponse.json");
            var mOBFOPResponse = JsonConvert.DeserializeObject<List<MOBFOPResponse>>(mOBFOPResponsejson);

            var mOBShoppingCartjson = TestDataGenerator.GetFileContent(@"TravelBankCredit\MOBShoppingCart.json");
            var mOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(mOBShoppingCartjson);

          //  var cMSContentMessagesjson = TestDataGenerator.GetFileContent(@"TravelBankCredit\CMSContentMessage.json");
          //  var cMSContentMessages = JsonConvert.DeserializeObject<List<List<CMSContentMessage>>>(cMSContentMessagesjson);


            return new object[] { mOBFOPTravelerBankRequest[0], Session[0], seatChangeState[0], Reservation[0],reservationDetail[0],ProfileResponse[0], mOBFOPResponse[0], mOBShoppingCart[0]};
        }

        public Object[] Set1_3()
        {
            var MOBFOPAcquirePaymentTokenRequestjson = TestDataGenerator.GetFileContent(@"GetPaymentToken\MOBFOPAcquirePaymentTokenRequest1.json");
            var mOBFOPAcquirePaymentTokenRequest = JsonConvert.DeserializeObject<List<MOBFOPAcquirePaymentTokenRequest>>(MOBFOPAcquirePaymentTokenRequestjson);



            var Sessionjson = TestDataGenerator.GetFileContent(@"GetPaymentToken\Session.json");
            var Session = JsonConvert.DeserializeObject<List<Session>>(Sessionjson);
            return new object[] { mOBFOPAcquirePaymentTokenRequest[0], Session[0] };
        }

        public Object[] Set2_1()
        {
            var MOBPersistFormofPaymentRequestjson = TestDataGenerator.GetFileContent(@"PersistFormofPaymentDetails\MOBPersistFormofPaymentRequest1.json");
            var mOBPersistFormofPaymentRequest = JsonConvert.DeserializeObject<List<MOBPersistFormofPaymentRequest>>(MOBPersistFormofPaymentRequestjson);


            var Sessionjson = TestDataGenerator.GetFileContent(@"PersistFormofPaymentDetails\Session.json");
            var Session = JsonConvert.DeserializeObject<List<Session>>(Sessionjson);

            var Reservationjson = TestDataGenerator.GetFileContent(@"PersistFormofPaymentDetails\Reservation.json");
            var Reservation = JsonConvert.DeserializeObject<List<Reservation>>(Reservationjson);


            var ProfileResponsejson = TestDataGenerator.GetFileContent(@"PersistFormofPaymentDetails\ProfileResponse.json");
            var ProfileResponse = JsonConvert.DeserializeObject<List<ProfileResponse>>(ProfileResponsejson);

            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"PersistFormofPaymentDetails\MOBShoppingCart.json");
            var MOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);

            var MOBFOPAcquirePaymentTokenResponsejson = TestDataGenerator.GetFileContent(@"PersistFormofPaymentDetails\MOBFOPAcquirePaymentTokenResponse1.json");
            var mOBFOPAcquirePaymentTokenResponse = JsonConvert.DeserializeObject<List<MOBFOPAcquirePaymentTokenResponse>>(MOBFOPAcquirePaymentTokenResponsejson);

            var ProfileFOPCreditCardResponsejson = TestDataGenerator.GetFileContent(@"PersistFormofPaymentDetails\ProfileFOPCreditCardResponse.json");
            var profileFOPCreditCardResponse = JsonConvert.DeserializeObject<List<ProfileFOPCreditCardResponse>>(ProfileFOPCreditCardResponsejson);

            var MOBVormetricKeysjson = TestDataGenerator.GetFileContent(@"PersistFormofPaymentDetails\MOBVormetricKeys.json");
            var mOBVormetricKeys = JsonConvert.DeserializeObject<List<MOBVormetricKeys>>(MOBVormetricKeysjson);

            var MOBCCAdStatementjson = TestDataGenerator.GetFileContent(@"PersistFormofPaymentDetails\MOBCCAdStatement.json");
            var mOBCCAdStatement = JsonConvert.DeserializeObject<List<MOBCCAdStatement>>(MOBCCAdStatementjson);

            var SeatChangeStatejson = TestDataGenerator.GetFileContent(@"PersistFormofPaymentDetails\SeatChangeState.json");
            var seatChangeState = JsonConvert.DeserializeObject<List<SeatChangeState>>(SeatChangeStatejson);



            return new object[] { mOBPersistFormofPaymentRequest[0], Session[0], Reservation[0], ProfileResponse[0], MOBShoppingCart[0], mOBFOPAcquirePaymentTokenResponse[0], profileFOPCreditCardResponse[0], mOBVormetricKeys[0], mOBCCAdStatement[0], seatChangeState[0] };
        }

        public Object[] Set3_1()
        {
            var MOBPersistFormofPaymentRequestjson = TestDataGenerator.GetFileContent(@"GetCreditCardToken\MOBPersistFormofPaymentRequest2.json");
            var mOBPersistFormofPaymentRequest = JsonConvert.DeserializeObject<List<MOBPersistFormofPaymentRequest>>(MOBPersistFormofPaymentRequestjson);


            var Sessionjson = TestDataGenerator.GetFileContent(@"GetCreditCardToken\Session.json");
            var Session = JsonConvert.DeserializeObject<List<Session>>(Sessionjson);

            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"GetCreditCardToken\MOBShoppingCart.json");
            var MOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);

            var Reservationjson = TestDataGenerator.GetFileContent(@"GetCreditCardToken\Reservation.json");
            var Reservation = JsonConvert.DeserializeObject<List<Reservation>>(Reservationjson);


            var MOBVormetricKeysjson = TestDataGenerator.GetFileContent(@"GetCreditCardToken\MOBVormetricKeys.json");
            var mOBVormetricKeys = JsonConvert.DeserializeObject<List<MOBVormetricKeys>>(MOBVormetricKeysjson);

            var MOBCCAdStatementjson = TestDataGenerator.GetFileContent(@"GetCreditCardToken\MOBCCAdStatement.json");
            var mOBCCAdStatement = JsonConvert.DeserializeObject<List<MOBCCAdStatement>>(MOBCCAdStatementjson);


            var MOBCSLContentMessagesResponsejson = TestDataGenerator.GetFileContent(@"GetCreditCardToken\MOBCSLContentMessagesResponse.json");
            var mOBCSLContentMessagesResponse = JsonConvert.DeserializeObject<List<MOBCSLContentMessagesResponse>>(MOBCSLContentMessagesResponsejson);

            var formofPaymentOptionsjson = TestDataGenerator.GetFileContent(@"GetCreditCardToken\FormofPaymentOption.json");
            var formofPaymentOptions = JsonConvert.DeserializeObject<List<List<FormofPaymentOption>>>(formofPaymentOptionsjson);

            return new object[] { mOBPersistFormofPaymentRequest[0], Session[0], MOBShoppingCart[0], Reservation[0], mOBVormetricKeys[0], mOBCCAdStatement[0], mOBCSLContentMessagesResponse[0], formofPaymentOptions[0] };
        }
    }
}
