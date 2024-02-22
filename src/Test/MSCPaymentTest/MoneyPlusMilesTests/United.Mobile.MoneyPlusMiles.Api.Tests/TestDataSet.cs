using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using United.Definition;
using United.Definition.Shopping;
using United.Persist.Definition.Shopping;
using United.Services.FlightShopping.Common.DisplayCart;
using United.Services.FlightShopping.Common.FlightReservation;

namespace United.Mobile.MoneyPlusMiles.Api.Tests
{
   public class TestDataSet
    {
        public Object[] Set1()
        {

            var GetMoneyPlusMilesRequestjson = TestDataGenerator.GetFileContent(@"GetMoneyPlusMilesRequest.json");
            var getMoneyPlusMilesRequest = JsonConvert.DeserializeObject<List<GetMoneyPlusMilesRequest>>(GetMoneyPlusMilesRequestjson);

            var sessionjson = TestDataGenerator.GetFileContent(@"Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);

            var cMSContentMessagesjson = TestDataGenerator.GetFileContent(@"CMSContentMessages.json");
            var cMSContentMessages = JsonConvert.DeserializeObject<List<List<CMSContentMessage>>>(cMSContentMessagesjson);

            var mOBShoppingCartjson = TestDataGenerator.GetFileContent(@"MOBShoppingCart.json");
            var mOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(mOBShoppingCartjson);

            var loadReservationAndDisplayCartResponsejson = TestDataGenerator.GetFileContent(@"LoadReservationAndDisplayCartResponse.json");
            var loadReservationAndDisplayCartResponse = JsonConvert.DeserializeObject<List<LoadReservationAndDisplayCartResponse>>(loadReservationAndDisplayCartResponsejson);

            var mOBFOPResponsejson = TestDataGenerator.GetFileContent(@"MOBFOPResponse.json");
            var mOBFOPResponse = JsonConvert.DeserializeObject<List<MOBFOPResponse>>(mOBFOPResponsejson);

            var profileResponsejson = TestDataGenerator.GetFileContent(@"ProfileResponse.json");
            var profileResponse = JsonConvert.DeserializeObject<List<ProfileResponse>>(profileResponsejson);


            return new object[] { getMoneyPlusMilesRequest[0], session[0], cMSContentMessages[0], mOBShoppingCart[0], loadReservationAndDisplayCartResponse[0], mOBFOPResponse[0], profileResponse[0] };

        }

        public Object[] Set2()
        {

            var applyMoneyPlusMilesOptionRequestjson = TestDataGenerator.GetFileContent(@"ApplyMoneyPlusMilesOptionRequest.json");
            var applyMoneyPlusMilesOptionRequest = JsonConvert.DeserializeObject<List<ApplyMoneyPlusMilesOptionRequest>>(applyMoneyPlusMilesOptionRequestjson);

            var sessionjson = TestDataGenerator.GetFileContent(@"Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);

            var cMSContentMessagesjson = TestDataGenerator.GetFileContent(@"CMSContentMessages.json");
            var cMSContentMessages = JsonConvert.DeserializeObject<List<List<CMSContentMessage>>>(cMSContentMessagesjson);

            var loadReservationAndDisplayCartResponsejson = TestDataGenerator.GetFileContent(@"LoadReservationAndDisplayCartResponse.json");
            var loadReservationAndDisplayCartResponse = JsonConvert.DeserializeObject<List<LoadReservationAndDisplayCartResponse>>(loadReservationAndDisplayCartResponsejson);

            var flightReservationResponsejson = TestDataGenerator.GetFileContent(@"FlightReservationResponse.json");
            var flightReservationResponse = JsonConvert.DeserializeObject<List<FlightReservationResponse>>(flightReservationResponsejson);

            var mOBShoppingCartjson = TestDataGenerator.GetFileContent(@"MOBShoppingCart.json");
            var mOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(mOBShoppingCartjson);

            var reservationjson = TestDataGenerator.GetFileContent(@"Reservation.json");
            var reservation = JsonConvert.DeserializeObject<List<Reservation>>(reservationjson);

            var mOBFOPResponsejson = TestDataGenerator.GetFileContent(@"MOBFOPResponse.json");
            var mOBFOPResponse = JsonConvert.DeserializeObject<List<MOBFOPResponse>>(mOBFOPResponsejson);

            var profileResponsejson = TestDataGenerator.GetFileContent(@"ProfileResponse.json");
            var profileResponse = JsonConvert.DeserializeObject<List<ProfileResponse>>(profileResponsejson);


            return new object[] { applyMoneyPlusMilesOptionRequest[0], session[0], cMSContentMessages[0], loadReservationAndDisplayCartResponse[0], flightReservationResponse[0], mOBShoppingCart[0], reservation[0],  mOBFOPResponse[0], profileResponse[0] };

        }
    }
}
