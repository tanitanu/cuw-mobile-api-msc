using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using United.Definition;
using United.Definition.FormofPayment;
using United.Definition.Shopping;
using United.Persist.Definition.Shopping;

namespace United.Mobile.Test.MSCCheckOut.Api.Tests
{
    public class TestDataSet
    {
        public Object[] Set1()
        {
            var MOBCheckOutRequestjson = TestDataGenerator.GetFileContent(@"CheckOut\MOBCheckOutRequest.json");
            var mOBCheckOutRequest = JsonConvert.DeserializeObject<List<MOBCheckOutRequest>>(MOBCheckOutRequestjson);

            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"CheckOut\MOBShoppingCart.json");
            var mOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);

            var Reservationjson = TestDataGenerator.GetFileContent(@"CheckOut\Reservation.json");
            var reservation = JsonConvert.DeserializeObject<List<Reservation>>(Reservationjson);

            var MOBApplyPromoCodeResponsejson = TestDataGenerator.GetFileContent(@"CheckOut\MOBApplyPromoCodeResponse.json");
            var mOBApplyPromoCodeResponse = JsonConvert.DeserializeObject<List<MOBApplyPromoCodeResponse>>(MOBApplyPromoCodeResponsejson);

            var MOBCSLContentMessagesResponsejson = TestDataGenerator.GetFileContent(@"CheckOut\MOBCSLContentMessagesResponse.json");
            var mOBCSLContentMessagesResponse = JsonConvert.DeserializeObject<List<MOBCSLContentMessagesResponse>>(MOBCSLContentMessagesResponsejson);

            var sessionjson = TestDataGenerator.GetFileContent(@"CheckOut\Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);

            var mOBCheckOutResponsejson = TestDataGenerator.GetFileContent(@"CheckOut\MOBCheckOutResponse.json");
            var mOBCheckOutResponse = JsonConvert.DeserializeObject<List<MOBCheckOutResponse>>(mOBCheckOutResponsejson);

            return new object[] { mOBCheckOutRequest[0], mOBShoppingCart[0], reservation[0], mOBApplyPromoCodeResponse[0], mOBCSLContentMessagesResponse[0], session[0], mOBCheckOutResponse[0] };
        }

        public Object[] Set2()
        {
            var MOBCheckOutRequestjson = TestDataGenerator.GetFileContent(@"CheckOut\MOBCheckOutRequest.json");
            var mOBCheckOutRequest = JsonConvert.DeserializeObject<List<MOBCheckOutRequest>>(MOBCheckOutRequestjson);

            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"CheckOut\MOBShoppingCart.json");
            var mOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);

            var Reservationjson = TestDataGenerator.GetFileContent(@"CheckOut\Reservation.json");
            var reservation = JsonConvert.DeserializeObject<List<Reservation>>(Reservationjson);

            var MOBApplyPromoCodeResponsejson = TestDataGenerator.GetFileContent(@"CheckOut\MOBApplyPromoCodeResponse.json");
            var mOBApplyPromoCodeResponse = JsonConvert.DeserializeObject<List<MOBApplyPromoCodeResponse>>(MOBApplyPromoCodeResponsejson);

            var MOBCSLContentMessagesResponsejson = TestDataGenerator.GetFileContent(@"CheckOut\MOBCSLContentMessagesResponse.json");
            var mOBCSLContentMessagesResponse = JsonConvert.DeserializeObject<List<MOBCSLContentMessagesResponse>>(MOBCSLContentMessagesResponsejson);

            var sessionjson = TestDataGenerator.GetFileContent(@"CheckOut\Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);

            var mOBCheckOutResponsejson = TestDataGenerator.GetFileContent(@"CheckOut\MOBCheckOutResponse.json");
            var mOBCheckOutResponse = JsonConvert.DeserializeObject<List<MOBCheckOutResponse>>(mOBCheckOutResponsejson);

            return new object[] { mOBCheckOutRequest[1], mOBShoppingCart[0], reservation[0], mOBApplyPromoCodeResponse[0], mOBCSLContentMessagesResponse[0], session[0], mOBCheckOutResponse[1] };
        }

        public Object[] Set3()
        {
            var MOBCheckOutRequestjson = TestDataGenerator.GetFileContent(@"CheckOut\MOBCheckOutRequest1.json");
            var mOBCheckOutRequest = JsonConvert.DeserializeObject<List<MOBCheckOutRequest>>(MOBCheckOutRequestjson);

            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"CheckOut\MOBShoppingCart.json");
            var mOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);

            var Reservationjson = TestDataGenerator.GetFileContent(@"CheckOut\Reservation.json");
            var reservation = JsonConvert.DeserializeObject<List<Reservation>>(Reservationjson);

            var MOBApplyPromoCodeResponsejson = TestDataGenerator.GetFileContent(@"CheckOut\MOBApplyPromoCodeResponse.json");
            var mOBApplyPromoCodeResponse = JsonConvert.DeserializeObject<List<MOBApplyPromoCodeResponse>>(MOBApplyPromoCodeResponsejson);

            var MOBCSLContentMessagesResponsejson = TestDataGenerator.GetFileContent(@"CheckOut\MOBCSLContentMessagesResponse.json");
            var mOBCSLContentMessagesResponse = JsonConvert.DeserializeObject<List<MOBCSLContentMessagesResponse>>(MOBCSLContentMessagesResponsejson);

            var sessionjson = TestDataGenerator.GetFileContent(@"CheckOut\Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);

            var mOBCheckOutResponsejson = TestDataGenerator.GetFileContent(@"CheckOut\MOBCheckOutResponse1.json");
            var mOBCheckOutResponse = JsonConvert.DeserializeObject<List<MOBCheckOutResponse>>(mOBCheckOutResponsejson);

            return new object[] { mOBCheckOutRequest[0], mOBShoppingCart[0], reservation[0], mOBApplyPromoCodeResponse[0], mOBCSLContentMessagesResponse[0], session[0], mOBCheckOutResponse[0] };
        }

        public Object[] Set1_1()
        {
            var MOBCheckOutRequestjson = TestDataGenerator.GetFileContent(@"CheckOut\MOBCheckOutRequest.json");
            var mOBCheckOutRequest = JsonConvert.DeserializeObject<List<MOBCheckOutRequest>>(MOBCheckOutRequestjson);

            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"CheckOut\MOBShoppingCart.json");
            var mOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);

            var Reservationjson = TestDataGenerator.GetFileContent(@"CheckOut\Reservation.json");
            var reservation = JsonConvert.DeserializeObject<List<Reservation>>(Reservationjson);

            var MOBApplyPromoCodeResponsejson = TestDataGenerator.GetFileContent(@"CheckOut\MOBApplyPromoCodeResponse.json");
            var mOBApplyPromoCodeResponse = JsonConvert.DeserializeObject<List<MOBApplyPromoCodeResponse>>(MOBApplyPromoCodeResponsejson);

            var MOBCSLContentMessagesResponsejson = TestDataGenerator.GetFileContent(@"CheckOut\MOBCSLContentMessagesResponse.json");
            var mOBCSLContentMessagesResponse = JsonConvert.DeserializeObject<List<MOBCSLContentMessagesResponse>>(MOBCSLContentMessagesResponsejson);

            var sessionjson = TestDataGenerator.GetFileContent(@"CheckOut\Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);

            var mOBCheckOutResponsejson = TestDataGenerator.GetFileContent(@"CheckOut\MOBCheckOutResponse.json");
            var mOBCheckOutResponse = JsonConvert.DeserializeObject<List<MOBCheckOutResponse>>(mOBCheckOutResponsejson);

            return new object[] { mOBCheckOutRequest[0], mOBShoppingCart[0], reservation[0], mOBApplyPromoCodeResponse[0], mOBCSLContentMessagesResponse[0], session[0], mOBCheckOutResponse[0] };
        }
    }
}


