using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using United.Definition;
using United.Persist.Definition.CCE;
using United.Persist.Definition.FOP;
using United.Persist.Definition.Merchandizing;
using United.Persist.Definition.Shopping;
using United.Service.Presentation.ReservationResponseModel;
//using United.Service.Presentation.RefundModel;
using United.Service.Presentation.SecurityResponseModel;
using United.Services.FlightShopping.Common.DisplayCart;
using United.Services.FlightShopping.Common.FlightReservation;

namespace United.Mobile.Test.MSCRegister.Api.Tests
{
   public class TestDataSet
    {
        public Object[] Set1()
        {

            var MOBRegisterOfferRequestjson = TestDataGenerator.GetFileContent(@"RegisterOffers\MOBRegisterOfferRequest.json");
            var mOBRegisterOfferRequest = JsonConvert.DeserializeObject<List<MOBRegisterOfferRequest>>(MOBRegisterOfferRequestjson);

            var Sessionjson = TestDataGenerator.GetFileContent(@"RegisterOffers\Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(Sessionjson);

            var ShopBookingDetailsResponsejson = TestDataGenerator.GetFileContent(@"RegisterOffers\ShopBookingDetailsResponse.json");
            var shopBookingDetailsResponse = JsonConvert.DeserializeObject<List<ShopBookingDetailsResponse>>(ShopBookingDetailsResponsejson);

            var CCEFlightReservationResponseByCartIdjson = TestDataGenerator.GetFileContent(@"RegisterOffers\CCEFlightReservationResponseByCartId.json");
            var cCEFlightReservationResponseByCartId = JsonConvert.DeserializeObject<List<CCEFlightReservationResponseByCartId>>(CCEFlightReservationResponseByCartIdjson);

            var FlightReservationResponsejson = TestDataGenerator.GetFileContent(@"RegisterOffers\FlightReservationResponse.json");
            var flightReservationResponse = JsonConvert.DeserializeObject<List<FlightReservationResponse>>(FlightReservationResponsejson);

            var PKDispenserKeyjson = TestDataGenerator.GetFileContent(@"RegisterOffers\PKDispenserKey.json");
            var pKDispenserKey = JsonConvert.DeserializeObject<List<PKDispenserKey>>(PKDispenserKeyjson);

            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"RegisterOffers\MOBShoppingCart.json");
            var mOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);

            var getOffersjson = TestDataGenerator.GetFileContent(@"RegisterOffers\GetOffers.json");
            var getOffers = JsonConvert.DeserializeObject<GetOffers>(getOffersjson);

            var getVendorOffersjson = TestDataGenerator.GetFileContent(@"RegisterOffers\GetVendorOffers.json");
            var getVendorOffers = JsonConvert.DeserializeObject<GetVendorOffers>(getVendorOffersjson);

            var reservationDetailjson = TestDataGenerator.GetFileContent(@"RegisterOffers\ReservationDetail.json");
            var reservationDetail = JsonConvert.DeserializeObject<List<ReservationDetail>>(reservationDetailjson);


            return new object[] { mOBRegisterOfferRequest[0], session[0], shopBookingDetailsResponse[0], cCEFlightReservationResponseByCartId[0], flightReservationResponse[0], pKDispenserKey[0], mOBShoppingCart[0], getOffers, getVendorOffers, reservationDetail[0] };
        }
        public Object[] Set2()
        {

            var MOBRegisterBagsRequestjson = TestDataGenerator.GetFileContent(@"RegisterBags\MOBRegisterBagsRequest.json");
            var mOBRegisterBagsRequest = JsonConvert.DeserializeObject<List<MOBRegisterBagsRequest>>(MOBRegisterBagsRequestjson);

            var Sessionjson = TestDataGenerator.GetFileContent(@"RegisterOffers\Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(Sessionjson);

            var ReservationDetailjson = TestDataGenerator.GetFileContent(@"RegisterBags\ReservationDetail.json");
            var reservationDetail = JsonConvert.DeserializeObject<List<ReservationDetail>>(ReservationDetailjson);

            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"RegisterBags\MOBShoppingCart.json");
            var mOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);

            var Reservationjson = TestDataGenerator.GetFileContent(@"RegisterBags\Reservation.json");
            var reservation = JsonConvert.DeserializeObject<List<Reservation>>(Reservationjson);

            var ProfileFOPCreditCardResponsejson = TestDataGenerator.GetFileContent(@"RegisterBags\ProfileFOPCreditCardResponse.json");
            var profileFOPCreditCardResponse = JsonConvert.DeserializeObject<List<ProfileFOPCreditCardResponse>>(ProfileFOPCreditCardResponsejson);

           
            var MOBPersistFormofPaymentResponsejson = TestDataGenerator.GetFileContent(@"RegisterBags\MOBPersistFormofPaymentResponse.json");
            var mOBPersistFormofPaymentResponse = JsonConvert.DeserializeObject<List<MOBPersistFormofPaymentResponse>>(MOBPersistFormofPaymentResponsejson);
           
        

            var ProfileResponsejson = TestDataGenerator.GetFileContent(@"RegisterBags\ProfileResponse.json");
            var profileResponse = JsonConvert.DeserializeObject<List<ProfileResponse>>(ProfileResponsejson);


            return new object[] { mOBRegisterBagsRequest[0], session[0], reservationDetail[0], mOBShoppingCart[0], reservation[0], profileFOPCreditCardResponse[0], mOBPersistFormofPaymentResponse[0],  profileResponse[0] };
        }
        public Object[] Set3()
        {

            var MOBRegisterSameDayChangeRequestjson = TestDataGenerator.GetFileContent(@"RegisterSameDayChange\MOBRegisterSameDayChangeRequest.json");
            var mOBRegisterSameDayChangeRequest = JsonConvert.DeserializeObject<List<MOBRegisterSameDayChangeRequest>>(MOBRegisterSameDayChangeRequestjson);

            var Sessionjson = TestDataGenerator.GetFileContent(@"RegisterSameDayChange\Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(Sessionjson);


            var ReservationDetailjson = TestDataGenerator.GetFileContent(@"RegisterSameDayChange\ReservationDetail.json");
            var reservationDetail = JsonConvert.DeserializeObject<List<ReservationDetail>>(ReservationDetailjson);

            var PKDispenserKeyjson = TestDataGenerator.GetFileContent(@"RegisterSameDayChange\PKDispenserKey.json");
            var pKDispenserKey = JsonConvert.DeserializeObject<List<PKDispenserKey>>(PKDispenserKeyjson);


            return new object[] { mOBRegisterSameDayChangeRequest[0], session[0], reservationDetail[0],  pKDispenserKey[0] };
        }
        public Object[] Set4()
        {

            var MOBRegisterCheckinSeatsRequestjson = TestDataGenerator.GetFileContent(@"RegisterCheckinSeats\MOBRegisterCheckinSeatsRequest.json");
            var mOBRegisterCheckinSeatsRequest = JsonConvert.DeserializeObject<List<MOBRegisterCheckinSeatsRequest>>(MOBRegisterCheckinSeatsRequestjson);

            var Sessionjson = TestDataGenerator.GetFileContent(@"RegisterCheckinSeats\Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(Sessionjson);


            var ReservationDetailjson = TestDataGenerator.GetFileContent(@"RegisterCheckinSeats\ReservationDetail.json");
            var reservationDetail = JsonConvert.DeserializeObject<List<ReservationDetail>>(ReservationDetailjson);

           
            var PKDispenserKeyjson = TestDataGenerator.GetFileContent(@"RegisterCheckinSeats\PKDispenserKey.json");
            var pKDispenserKey = JsonConvert.DeserializeObject<List<PKDispenserKey>>(PKDispenserKeyjson);

            var Reservationjson = TestDataGenerator.GetFileContent(@"RegisterCheckinSeats\Reservation.json");
            var reservation = JsonConvert.DeserializeObject<List<Reservation>>(Reservationjson);

            var LoadReservationAndDisplayCartResponsejson = TestDataGenerator.GetFileContent(@"RegisterCheckinSeats\LoadReservationAndDisplayCartResponse.json");
            var loadReservationAndDisplayCartResponse = JsonConvert.DeserializeObject<List<LoadReservationAndDisplayCartResponse>>(LoadReservationAndDisplayCartResponsejson);

            var ProfileResponsejson = TestDataGenerator.GetFileContent(@"RegisterCheckinSeats\ProfileResponse.json");
            var profileResponse = JsonConvert.DeserializeObject<List<ProfileResponse>>(ProfileResponsejson);


          

            return new object[] { mOBRegisterCheckinSeatsRequest[0], session[0], reservationDetail[0],  pKDispenserKey[0], reservation[0], loadReservationAndDisplayCartResponse[0],profileResponse[0]};
        }

        public Object[] Set5()
        {

            var MOBRegisterOfferRequestjson = TestDataGenerator.GetFileContent(@"RegisterOffers\MOBRegisterOfferRequest1.json");
            var mOBRegisterOfferRequest = JsonConvert.DeserializeObject<List<MOBRegisterOfferRequest>>(MOBRegisterOfferRequestjson);

            var Sessionjson = TestDataGenerator.GetFileContent(@"RegisterOffers\Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(Sessionjson);

            var ShopBookingDetailsResponsejson = TestDataGenerator.GetFileContent(@"RegisterOffers\ShopBookingDetailsResponse.json");
            var shopBookingDetailsResponse = JsonConvert.DeserializeObject<List<ShopBookingDetailsResponse>>(ShopBookingDetailsResponsejson);

            var CCEFlightReservationResponseByCartIdjson = TestDataGenerator.GetFileContent(@"RegisterOffers\CCEFlightReservationResponseByCartId.json");
            var cCEFlightReservationResponseByCartId = JsonConvert.DeserializeObject<List<CCEFlightReservationResponseByCartId>>(CCEFlightReservationResponseByCartIdjson);

            var FlightReservationResponsejson = TestDataGenerator.GetFileContent(@"RegisterOffers\FlightReservationResponse.json");
            var flightReservationResponse = JsonConvert.DeserializeObject<List<FlightReservationResponse>>(FlightReservationResponsejson);

            var PKDispenserKeyjson = TestDataGenerator.GetFileContent(@"RegisterOffers\PKDispenserKey.json");
            var pKDispenserKey = JsonConvert.DeserializeObject<List<PKDispenserKey>>(PKDispenserKeyjson);

            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"RegisterOffers\MOBShoppingCart.json");
            var mOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);

            var getOffersjson = TestDataGenerator.GetFileContent(@"RegisterOffers\GetOffers.json");
            var getOffers = JsonConvert.DeserializeObject<GetOffers>(getOffersjson);

            var getVendorOffersjson = TestDataGenerator.GetFileContent(@"RegisterOffers\GetVendorOffers.json");
            var getVendorOffers = JsonConvert.DeserializeObject<GetVendorOffers>(getVendorOffersjson);

            var reservationDetailjson = TestDataGenerator.GetFileContent(@"RegisterOffers\ReservationDetail.json");
            var reservationDetail = JsonConvert.DeserializeObject<List<ReservationDetail>>(reservationDetailjson);


            return new object[] { mOBRegisterOfferRequest[0], session[0], shopBookingDetailsResponse[0], cCEFlightReservationResponseByCartId[0], flightReservationResponse[0], pKDispenserKey[0], mOBShoppingCart[0], getOffers, getVendorOffers, reservationDetail[0] };
        }


        public Object[] Set1_1()
        {

            var MOBRegisterOfferRequestjson = TestDataGenerator.GetFileContent(@"RegisterOffers\MOBRegisterOfferRequest2.json");
            var mOBRegisterOfferRequest = JsonConvert.DeserializeObject<List<MOBRegisterOfferRequest>>(MOBRegisterOfferRequestjson);

            var Sessionjson = TestDataGenerator.GetFileContent(@"RegisterOffers\Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(Sessionjson);

            var ShopBookingDetailsResponsejson = TestDataGenerator.GetFileContent(@"RegisterOffers\ShopBookingDetailsResponse.json");
            var shopBookingDetailsResponse = JsonConvert.DeserializeObject<List<ShopBookingDetailsResponse>>(ShopBookingDetailsResponsejson);

            var CCEFlightReservationResponseByCartIdjson = TestDataGenerator.GetFileContent(@"RegisterOffers\CCEFlightReservationResponseByCartId.json");
            var cCEFlightReservationResponseByCartId = JsonConvert.DeserializeObject<List<CCEFlightReservationResponseByCartId>>(CCEFlightReservationResponseByCartIdjson);

            var FlightReservationResponsejson = TestDataGenerator.GetFileContent(@"RegisterOffers\FlightReservationResponse.json");
            var flightReservationResponse = JsonConvert.DeserializeObject<List<FlightReservationResponse>>(FlightReservationResponsejson);

            var PKDispenserKeyjson = TestDataGenerator.GetFileContent(@"RegisterOffers\PKDispenserKey.json");
            var pKDispenserKey = JsonConvert.DeserializeObject<List<PKDispenserKey>>(PKDispenserKeyjson);

            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"RegisterOffers\MOBShoppingCart.json");
            var mOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);

            var getOffersjson = TestDataGenerator.GetFileContent(@"RegisterOffers\GetOffers.json");
            var getOffers = JsonConvert.DeserializeObject<GetOffers>(getOffersjson);

            var getVendorOffersjson = TestDataGenerator.GetFileContent(@"RegisterOffers\GetVendorOffers.json");
            var getVendorOffers = JsonConvert.DeserializeObject<GetVendorOffers>(getVendorOffersjson);

            var reservationDetailjson = TestDataGenerator.GetFileContent(@"RegisterOffers\ReservationDetail.json");
            var reservationDetail = JsonConvert.DeserializeObject<List<ReservationDetail>>(reservationDetailjson);


            return new object[] { mOBRegisterOfferRequest[0], session[0], shopBookingDetailsResponse[0], cCEFlightReservationResponseByCartId[0], flightReservationResponse[0], pKDispenserKey[0], mOBShoppingCart[0], getOffers, getVendorOffers, reservationDetail[0] };
        }

    }
    }

