using System;
using System.Collections.Generic;
using System.Text;
using United.Definition;
using United.Definition.FormofPayment;
using United.Definition.SeatCSL30;
using United.Mobile.Model;
using United.Mobile.Model.Common;
//using United.Mobile.Model.MSCRegister;
using United.Persist.Definition.CCE;
//using United.Mobile.Model.Shopping;
using United.Persist.Definition.SeatChange;
//using United.Mobile.Model.Product;
//using United.Mobile.Model.PromoCode;

//using United.Mobile.Model.Shopping.FormofPayment;
//using United.Mobile.Model.TravelCredit;
using United.Persist.Definition.Shopping;
using United.Service.Presentation.SecurityResponseModel;
using United.Services.FlightShopping.Common.FlightReservation;
using United.Definition.Shopping;
using United.Definition.Shopping.UnfinishedBooking;
using United.Definition.Shopping.Bundles;
using United.Service.Presentation.ProductModel;
using United.Persist.Definition.FOP;
using Newtonsoft.Json;
using ProfileResponse = United.Persist.Definition.Shopping.ProfileResponse;
using United.Persist.Definition.Merchandizing;
using United.Service.Presentation.ReservationResponseModel;
using United.Mobile.Model.DynamoDb.Common;

namespace United.Mobile.Test.Product.Api.Tests
{
    public class TestDataSet
    {
        public Object[] Set1()
        {

            var MOBRegisterOfferRequestjson = TestDataGenerator.GetFileContent(@"RegisterOffersForBooking\MOBRegisterOfferRequest.json");
            var MOBRegisterOfferRequest = JsonConvert.DeserializeObject<List<MOBRegisterOfferRequest>>(MOBRegisterOfferRequestjson);

            var sessionjson = TestDataGenerator.GetFileContent(@"RegisterOffersForBooking\Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);

            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"RegisterOffersForBooking\MOBShoppingCart.json");
            var MOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);

            var MOBApplyPromoCodeResponsejson = TestDataGenerator.GetFileContent(@"RegisterOffersForBooking\MOBApplyPromoCodeResponse.json");
            var MOBApplyPromoCode = JsonConvert.DeserializeObject<MOBApplyPromoCodeResponse>(MOBApplyPromoCodeResponsejson);

            var FlightReservationResponsejson = TestDataGenerator.GetFileContent(@"RegisterOffersForBooking\FlightReservationResponse.json");
            var FlightReservationResponse = JsonConvert.DeserializeObject<List<FlightReservationResponse>>(FlightReservationResponsejson);

            var GetOffersjson = TestDataGenerator.GetFileContent(@"RegisterOffersForBooking\GetOffers.json");
            var GetOffers = JsonConvert.DeserializeObject<List<GetOffers>>(GetOffersjson);

            var MOBBookingBundlesResponsejson = TestDataGenerator.GetFileContent(@"RegisterOffersForBooking\MOBBookingBundlesResponse.json");
            var MOBBookingBundlesResponse = JsonConvert.DeserializeObject<List<MOBBookingBundlesResponse>>(MOBBookingBundlesResponsejson);

            var Reservationjson = TestDataGenerator.GetFileContent(@"RegisterOffersForBooking\Reservation.json");
            var Reservation = JsonConvert.DeserializeObject<List<Reservation>>(Reservationjson);

            var VendorOffersjson = TestDataGenerator.GetFileContent(@"RegisterOffersForBooking\VendorOffers.json");
            var VendorOffers = JsonConvert.DeserializeObject<List<GetVendorOffers>>(VendorOffersjson);

            var SelectTripjson = TestDataGenerator.GetFileContent(@"RegisterOffersForBooking\SelectTrip.json");
            var SelectTrip = JsonConvert.DeserializeObject<List<SelectTrip>>(SelectTripjson);

            var MOBFOPTravelerCertificateResponsejson = TestDataGenerator.GetFileContent(@"RegisterOffersForBooking\MOBFOPTravelerCertificateResponse.json");
            var MOBFOPTravelerCertificate = JsonConvert.DeserializeObject<List<MOBFOPTravelerCertificateResponse>>(MOBFOPTravelerCertificateResponsejson);

            var ReservationDetailjson = TestDataGenerator.GetFileContent(@"RegisterOffersForBooking\ReservationDetail.json");
            var reservationDetail = JsonConvert.DeserializeObject<List<ReservationDetail>>(ReservationDetailjson);

            var CCEFlightReservationResponseByCartIdjson = TestDataGenerator.GetFileContent(@"RegisterOffersForBooking\CCEFlightReservationResponseByCartId.json");
            var CCEFlightReservationResponseByCartId = JsonConvert.DeserializeObject<List<CCEFlightReservationResponseByCartId>>(CCEFlightReservationResponseByCartIdjson);

            var PKDispenserKeyjson = TestDataGenerator.GetFileContent(@"RegisterOffersForBooking\PKDispenserKey.json");
            var PKDispenserKey = JsonConvert.DeserializeObject<List<PKDispenserKey>>(PKDispenserKeyjson);
            
            var eligibleFormofPaymentsjson = TestDataGenerator.GetFileContent(@"RegisterOffersForBooking\EligibleFormofPayments.json");
            var eligibleFormofPayments = JsonConvert.DeserializeObject<List<List<FormofPaymentOption>>>(eligibleFormofPaymentsjson);

            var  shopBookingDetailsResponseJson = TestDataGenerator.GetFileContent(@"RegisterOffersForBooking\ShopBookingDetailsResponse.json");
            var shopBookingDetailsResponse = JsonConvert.DeserializeObject<ShopBookingDetailsResponse>(shopBookingDetailsResponseJson);


            //var MOBBookingRegisterOfferResponsejson = TestDataGenerator.GetFileContent(@"RegisterOffersForBookingTestData\MOBBookingRegisterOfferResponse.json");
            //var MOBBookingRegisterOfferResponse = JsonConvert.DeserializeObject<List<MOBBookingRegisterOfferResponse>>(MOBBookingRegisterOfferResponsejson);


            return new object[] { MOBRegisterOfferRequest[0], session[0],
                MOBShoppingCart[0], MOBApplyPromoCode, FlightReservationResponse[0], GetOffers[0], MOBBookingBundlesResponse[0],
                Reservation[0], VendorOffers[0], SelectTrip[0], MOBFOPTravelerCertificate[0], reservationDetail[0], 
                CCEFlightReservationResponseByCartId[0], PKDispenserKey[0], eligibleFormofPayments[0],shopBookingDetailsResponse };

        }
        public Object[] Set2()
        {
            var MOBRegisterOfferRequestjson = TestDataGenerator.GetFileContent(@"RegisterOffersForReshop\MOBRegisterOfferRequest.json");
            var MOBRegisterOfferRequest = JsonConvert.DeserializeObject<List<MOBRegisterOfferRequest>>(MOBRegisterOfferRequestjson);

            var sessionjson = TestDataGenerator.GetFileContent(@"RegisterOffersForReshop\Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);

            var Reservationjson = TestDataGenerator.GetFileContent(@"RegisterOffersForReshop\Reservation.json");
            var reservation = JsonConvert.DeserializeObject<List<Reservation>>(Reservationjson);



            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"RegisterOffersForReshop\MOBShoppingCart.json");
            var mOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);

            var GetOffersjson = TestDataGenerator.GetFileContent(@"RegisterOffersForReshop\GetOffers.json");
            var GetOffers = JsonConvert.DeserializeObject<List<GetOffers>>(GetOffersjson);


            var ShopBookingDetailsResponsejson = TestDataGenerator.GetFileContent(@"RegisterOffersForReshop\ShopBookingDetailsResponse.json");
            var ShopBookingDetailsResponse = JsonConvert.DeserializeObject<List<ShopBookingDetailsResponse>>(ShopBookingDetailsResponsejson);

            var FlightReservationResponsejson = TestDataGenerator.GetFileContent(@"RegisterOffersForReshop\FlightReservationResponse.json");
            var FlightReservationResponse = JsonConvert.DeserializeObject<List<FlightReservationResponse>>(FlightReservationResponsejson);

            var VendorOffersjson = TestDataGenerator.GetFileContent(@"RegisterOffersForReshop\VendorOffers.json");
            var VendorOffers  = JsonConvert.DeserializeObject<List<GetVendorOffers>>(VendorOffersjson);


            var PKDispenserKeyjson = TestDataGenerator.GetFileContent(@"RegisterOffersForReshop\PKDispenserKey.json");
            var PKDispenserKeyResponse = JsonConvert.DeserializeObject<List<PKDispenserKey>>(PKDispenserKeyjson);

            var MOBFOPTravelerCertificateResponsejson = TestDataGenerator.GetFileContent(@"RegisterOffersForReshop\MOBFOPTravelerCertificateResponse.json");
            var MOBFOPTravelerCertificateResponse = JsonConvert.DeserializeObject<MOBFOPTravelerCertificateResponse>(MOBFOPTravelerCertificateResponsejson);


           

            var MOBSHOPReservationjson = TestDataGenerator.GetFileContent(@"RegisterOffersForReshop\MOBSHOPReservation.json");
            var MOBSHOPReservation = JsonConvert.DeserializeObject<List<MOBSHOPReservation>>(MOBSHOPReservationjson);

            //var MOBBookingRegisterOfferResponsejson = TestDataGenerator.GetFileContent(@"RegisterOffersForReshop\MOBRegisterOfferResponse.json");
            //var MOBBookingRegisterOfferResponse = JsonConvert.DeserializeObject<List<MOBBookingRegisterOfferResponse>>(MOBBookingRegisterOfferResponsejson);


            return new object[] { MOBRegisterOfferRequest[0],  session[0], reservation[0], mOBShoppingCart[0], GetOffers[0], ShopBookingDetailsResponse[0], FlightReservationResponse[0], VendorOffers[0], PKDispenserKeyResponse[0], MOBFOPTravelerCertificateResponse,  MOBSHOPReservation[0] };
        }
        public Object[] Set3()
        {
            var MOBRegisterSeatsRequestjson = TestDataGenerator.GetFileContent(@"RegisterSeatsForBooking\MOBRegisterSeatsRequest.json");
            var MOBRegisterSeatsRequest = JsonConvert.DeserializeObject<List<MOBRegisterSeatsRequest>>(MOBRegisterSeatsRequestjson);

            var sessionjson = TestDataGenerator.GetFileContent(@"RegisterSeatsForBooking\Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);

            var persistSelectSeatsResponsejson = TestDataGenerator.GetFileContent(@"RegisterSeatsForBooking\persistSelectSeatsResponse.json");
            var persistSelectSeatsResponse = JsonConvert.DeserializeObject<List<SelectSeats>>(persistSelectSeatsResponsejson);

            var Reservationjson = TestDataGenerator.GetFileContent(@"RegisterSeatsForBooking\Reservation.json");
            var Reservation = JsonConvert.DeserializeObject<List<Reservation>>(Reservationjson);

            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"RegisterSeatsForBooking\MOBShoppingCart.json");
            var MOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);


            var MOBApplyPromoCodeResponsejson = TestDataGenerator.GetFileContent(@"RegisterSeatsForBooking\MOBApplyPromoCodeResponse.json");
            var MOBApplyPromoCodeResponse = JsonConvert.DeserializeObject<List<MOBApplyPromoCodeResponse>>(MOBApplyPromoCodeResponsejson);

            var FlightReservationResponsejson = TestDataGenerator.GetFileContent(@"RegisterSeatsForBooking\FlightReservationResponse.json");
            var FlightReservationResponse = JsonConvert.DeserializeObject<List<FlightReservationResponse>>(FlightReservationResponsejson);

            var ProfileResponsejson = TestDataGenerator.GetFileContent(@"RegisterSeatsForBooking\ProfileResponse.json");
            var ProfileResponse = JsonConvert.DeserializeObject<List<ProfileResponse>>(ProfileResponsejson);

            var ProfileFOPCreditCardResponsejson = TestDataGenerator.GetFileContent(@"RegisterSeatsForBooking\ProfileFOPCreditCardResponse.json");
            var ProfileFOPCreditCardResponse = JsonConvert.DeserializeObject<List<ProfileFOPCreditCardResponse>>(ProfileFOPCreditCardResponsejson);


            var MOBFutureFlightCreditResponsejson = TestDataGenerator.GetFileContent(@"RegisterSeatsForBooking\MOBFutureFlightCreditResponse.json");
            var MOBFutureFlightCreditResponse = JsonConvert.DeserializeObject<List<MOBFutureFlightCreditResponse>>(MOBFutureFlightCreditResponsejson);

            var CSLContentMessagesResponsejson = TestDataGenerator.GetFileContent(@"RegisterSeatsForBooking\CSLContentMessagesResponse.json");
            var CSLContentMessagesResponse = JsonConvert.DeserializeObject<List<MOBCSLContentMessagesResponse>>(CSLContentMessagesResponsejson);

            var MOBFOPTravelerCertificateResponsejson = TestDataGenerator.GetFileContent(@"RegisterSeatsForBooking\MOBFOPTravelerCertificateResponse.json");
            var MOBFOPTravelerCertificateResponse = JsonConvert.DeserializeObject<MOBFOPTravelerCertificateResponse>(MOBFOPTravelerCertificateResponsejson);

            var SeatChangeStatejson = TestDataGenerator.GetFileContent(@"RegisterSeatsForBooking\SeatChangeState.json");
            var SeatChangeState = JsonConvert.DeserializeObject<List<SeatChangeState>>(SeatChangeStatejson);

            var MOBBookingRegisterSeatsResponsejson = TestDataGenerator.GetFileContent(@"RegisterSeatsForBooking\MOBBookingRegisterSeatsResponse.json");
            var MOBBookingRegisterSeatsResponse = JsonConvert.DeserializeObject<List<MOBBookingRegisterSeatsResponse>>(MOBBookingRegisterSeatsResponsejson);

            var CCEFlightReservationResponseByCartIdjson = TestDataGenerator.GetFileContent(@"RegisterSeatsForBooking\CCEFlightReservationResponseByCartId.json");
            var CCEFlightReservationResponseByCartId = JsonConvert.DeserializeObject<List<CCEFlightReservationResponseByCartId>>(CCEFlightReservationResponseByCartIdjson);


            return new object[] { MOBRegisterSeatsRequest[0],  session[0], persistSelectSeatsResponse[0], Reservation[0],
                MOBShoppingCart[0], MOBApplyPromoCodeResponse[0], FlightReservationResponse[0], ProfileResponse[0],
                ProfileFOPCreditCardResponse[0], MOBFutureFlightCreditResponse[0],CSLContentMessagesResponse[0], MOBFOPTravelerCertificateResponse,
                SeatChangeState[0], MOBBookingRegisterSeatsResponse[0],CCEFlightReservationResponseByCartId[0]};
        }
        public Object[] Set4()
        {
            var MOBRegisterSeatsRequestjson = TestDataGenerator.GetFileContent(@"RegisterSeatsForReshop\MOBRegisterSeatsRequest.json");
            var MOBRegisterSeatsRequest = JsonConvert.DeserializeObject<List<MOBRegisterSeatsRequest>>(MOBRegisterSeatsRequestjson);

            var sessionjson = TestDataGenerator.GetFileContent(@"RegisterSeatsForReshop\Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);

            var SelectSeatsjson = TestDataGenerator.GetFileContent(@"RegisterSeatsForReshop\SelectSeats.json");
            var SelectSeats = JsonConvert.DeserializeObject<List<SelectSeats>>(SelectSeatsjson);


            // var Reservationjson = TestDataGenerator.GetFileContent(@"RegisterSeatsForReshop\Reservation.json");
            //var Reservation = JsonConvert.DeserializeObject<List<Reservation>>(Reservationjson);

            var Reservation = TestDataGenerator.GetXmlData<Reservation>(@"RegisterSeatsForReshop\E00A45EDC7A94840B04A5A13D7440B9AUnited.Persist.Definition.Shopping.Reservation.xml");

            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"RegisterSeatsForReshop\MOBShoppingCart.json");
            var MOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);

            var SelectTripjson = TestDataGenerator.GetFileContent(@"RegisterSeatsForReshop\SelectTrip.json");
            var SelectTrip = JsonConvert.DeserializeObject<List<SelectTrip>>(SelectTripjson);


            var MOBApplyPromoCodeResponsejson = TestDataGenerator.GetFileContent(@"RegisterSeatsForReshop\MOBApplyPromoCodeResponse.json");
            var MOBApplyPromoCodeResponse = JsonConvert.DeserializeObject<List<MOBApplyPromoCodeResponse>>(MOBApplyPromoCodeResponsejson);

            var FlightReservationResponsejson = TestDataGenerator.GetFileContent(@"RegisterSeatsForReshop\FlightReservationResponse.json");
            var FlightReservationResponse = JsonConvert.DeserializeObject<List<FlightReservationResponse>>(FlightReservationResponsejson);

            var MOBFOPTravelerCertificateResponsejson = TestDataGenerator.GetFileContent(@"RegisterSeatsForReshop\MOBFOPTravelerCertificateResponse.json");
            var MOBFOPTravelerCertificateResponse = JsonConvert.DeserializeObject<MOBFOPTravelerCertificateResponse>(MOBFOPTravelerCertificateResponsejson);

            var MOBReshopRegisterSeatsResponsejson = TestDataGenerator.GetFileContent(@"RegisterSeatsForReshop\MOBReshopRegisterSeatsResponse.json");
            var MOBReshopRegisterSeatsResponse = JsonConvert.DeserializeObject<List<MOBReshopRegisterSeatsResponse>>(MOBReshopRegisterSeatsResponsejson);


            return new object[] { MOBRegisterSeatsRequest[0], session[0], SelectSeats[0], Reservation, MOBShoppingCart[0], SelectTrip[0], MOBApplyPromoCodeResponse[0], FlightReservationResponse[0], MOBFOPTravelerCertificateResponse, MOBReshopRegisterSeatsResponse[0] };
        }

        public Object[] Set5()
        {
            var MOBRemoveAncillaryOfferRequestjson = TestDataGenerator.GetFileContent(@"RemoveAncillaryOffer\MOBRemoveAncillaryOfferRequest.json");
            var MOBRemoveAncillaryOfferRequest = JsonConvert.DeserializeObject<List<MOBRemoveAncillaryOfferRequest>>(MOBRemoveAncillaryOfferRequestjson);

            var sessionjson = TestDataGenerator.GetFileContent(@"RemoveAncillaryOffer\Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);

            var Reservationjson = TestDataGenerator.GetFileContent(@"RemoveAncillaryOffer\Reservation.json");
            var Reservation = JsonConvert.DeserializeObject<List<Reservation>>(Reservationjson);


            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"RemoveAncillaryOffer\MOBShoppingCart.json");
            var MOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);

            var ShopBookingDetailsResponsejson = TestDataGenerator.GetFileContent(@"RemoveAncillaryOffer\ShopBookingDetailsResponse.json");
            var ShopBookingDetailsResponse = JsonConvert.DeserializeObject<List<ShopBookingDetailsResponse>>(ShopBookingDetailsResponsejson);

            var CCEFlightReservationResponseByCartIdjson = TestDataGenerator.GetFileContent(@"RemoveAncillaryOffer\CCEFlightReservationResponseByCartId.json");
            var CCEFlightReservationResponseByCartId = JsonConvert.DeserializeObject<List<CCEFlightReservationResponseByCartId>>(CCEFlightReservationResponseByCartIdjson);

            var FlightReservationResponsejson = TestDataGenerator.GetFileContent(@"RemoveAncillaryOffer\FlightReservationResponse.json");
            var FlightReservationResponse = JsonConvert.DeserializeObject<List<FlightReservationResponse>>(FlightReservationResponsejson);

            var MOBBookingBundlesResponsejson = TestDataGenerator.GetFileContent(@"RemoveAncillaryOffer\MOBBookingBundlesResponse.json");
            var MOBBookingBundlesResponse = JsonConvert.DeserializeObject<List<MOBBookingBundlesResponse>>(MOBBookingBundlesResponsejson);

            var PKDispenserResponsejson = TestDataGenerator.GetFileContent(@"RemoveAncillaryOffer\PKDispenserResponse.json");
            var PKDispenserResponse = JsonConvert.DeserializeObject<List<PKDispenserResponse>>(PKDispenserResponsejson);

            var GetOffersjson = TestDataGenerator.GetFileContent(@"RemoveAncillaryOffer\GetOffers.json");
            var GetOffers = JsonConvert.DeserializeObject<List<GetOffers>>(GetOffersjson);

            var VendorOffersjson = TestDataGenerator.GetFileContent(@"RemoveAncillaryOffer\VendorOffers.json");
            var VendorOffers = JsonConvert.DeserializeObject<List<GetVendorOffers>>(VendorOffersjson);


            var MOBCSLContentMessagesResponsejson = TestDataGenerator.GetFileContent(@"RemoveAncillaryOffer\MOBCSLContentMessagesResponse.json");
            var MOBCSLContentMessagesResponse = JsonConvert.DeserializeObject<List<MOBCSLContentMessagesResponse>>(MOBCSLContentMessagesResponsejson);


            //var MOBBookingRegisterSeatsResponsejson = TestDataGenerator.GetFileContent(@"RemoveAncillaryOffer\ MOBBookingRegisterSeatsResponse.json");
            //var MOBBookingRegisterSeatsResponse = JsonConvert.DeserializeObject<List<MOBBookingRegisterOfferResponse>>(MOBBookingRegisterSeatsResponsejson);

          //  var SelectSeatsjson = TestDataGenerator.GetFileContent(@"RemoveAncillaryOffer\SelectSeats.json");
          //  var SelectSeats = JsonConvert.DeserializeObject<List<SelectSeats>>(SelectSeatsjson);


            return new object[] { MOBRemoveAncillaryOfferRequest[0],  session[0], Reservation[0], MOBShoppingCart[0], ShopBookingDetailsResponse[0], CCEFlightReservationResponseByCartId[0], FlightReservationResponse[0], MOBBookingBundlesResponse[0], PKDispenserResponse[0], GetOffers[0], VendorOffers[0], MOBCSLContentMessagesResponse[0]};
        }

        public Object[] Set6()
        {
            var MOBClearCartOnBackNavigationRequestjson = TestDataGenerator.GetFileContent(@"ClearCartOnBackNavigation\MOBClearCartOnBackNavigationRequest.json");
            var MOBClearCartOnBackNavigationRequest = JsonConvert.DeserializeObject<List<MOBClearCartOnBackNavigationRequest>>(MOBClearCartOnBackNavigationRequestjson);

            var sessionjson = TestDataGenerator.GetFileContent(@"ClearCartOnBackNavigation\Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);

            var Reservationjson = TestDataGenerator.GetFileContent(@"ClearCartOnBackNavigation\Reservation.json");
            var Reservation = JsonConvert.DeserializeObject<List<Reservation>>(Reservationjson);


            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"ClearCartOnBackNavigation\MOBShoppingCart.json");
            var MOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);

            var MOBSeatMapCSL30json = TestDataGenerator.GetFileContent(@"ClearCartOnBackNavigation\MOBSeatMapCSL30.json");
            var MOBSeatMapCSL30 = JsonConvert.DeserializeObject<List<MOBSeatMapCSL30>>(MOBSeatMapCSL30json);


            var ShoppingResponsejson = TestDataGenerator.GetFileContent(@"ClearCartOnBackNavigation\ShoppingResponse.json");
            var ShoppingResponse = JsonConvert.DeserializeObject<List<ShoppingResponse>>(ShoppingResponsejson);

            var FlightSeatMapDetailjson = TestDataGenerator.GetFileContent(@"ClearCartOnBackNavigation\FlightSeatMapDetail.json");
            var FlightSeatMapDetail = JsonConvert.DeserializeObject<List<FlightSeatMapDetail>>(FlightSeatMapDetailjson);

            var MOBBookingBundlesResponsejson = TestDataGenerator.GetFileContent(@"ClearCartOnBackNavigation\MOBBookingBundlesResponse.json");
            var MOBBookingBundlesResponse = JsonConvert.DeserializeObject<List<MOBBookingBundlesResponse>>(MOBBookingBundlesResponsejson);

            var GetOffersjson = TestDataGenerator.GetFileContent(@"ClearCartOnBackNavigation\GetOffers.json");
            var GetOffers = JsonConvert.DeserializeObject<List<GetOffers>>(GetOffersjson);

            var VendorOffersjson = TestDataGenerator.GetFileContent(@"ClearCartOnBackNavigation\VendorOffers.json");
            var VendorOffers = JsonConvert.DeserializeObject<List<GetVendorOffers>>(VendorOffersjson);


            var CCEFlightReservationResponseByCartIdjson = TestDataGenerator.GetFileContent(@"ClearCartOnBackNavigation\CCEFlightReservationResponseByCartId.json");
            var CCEFlightReservationResponseByCartId = JsonConvert.DeserializeObject<List<CCEFlightReservationResponseByCartId>>(CCEFlightReservationResponseByCartIdjson);


            var FlightReservationResponsejson = TestDataGenerator.GetFileContent(@"ClearCartOnBackNavigation\FlightReservationResponse.json");
            var FlightReservationResponse = JsonConvert.DeserializeObject<List<FlightReservationResponse>>(FlightReservationResponsejson);


            var PKDispenserResponsejson = TestDataGenerator.GetFileContent(@"ClearCartOnBackNavigation\PKDispenserResponse.json");
            var PKDispenserResponse = JsonConvert.DeserializeObject<List<PKDispenserResponse>>(PKDispenserResponsejson);


            var CSLShopRequestjson = TestDataGenerator.GetFileContent(@"ClearCartOnBackNavigation\CSLShopRequest.json");
            var CSLShopRequest = JsonConvert.DeserializeObject<List<CSLShopRequest>>(CSLShopRequestjson);


            var MOBFutureFlightCreditResponsejson = TestDataGenerator.GetFileContent(@"ClearCartOnBackNavigation\MOBFutureFlightCreditResponse.json");
            var MOBFutureFlightCreditResponse = JsonConvert.DeserializeObject<List<MOBFutureFlightCreditResponse>>(MOBFutureFlightCreditResponsejson);

            var MOBCSLContentMessagesResponsejson = TestDataGenerator.GetFileContent(@"ClearCartOnBackNavigation\MOBCSLContentMessagesResponse.json");
            var MOBCSLContentMessagesResponse = JsonConvert.DeserializeObject<List<MOBCSLContentMessagesResponse>>(MOBCSLContentMessagesResponsejson);


            var MOBApplyPromoCodeResponsejson = TestDataGenerator.GetFileContent(@"ClearCartOnBackNavigation\MOBApplyPromoCodeResponse.json");
            var MOBApplyPromoCodeResponse = JsonConvert.DeserializeObject<List<MOBApplyPromoCodeResponse>>(MOBApplyPromoCodeResponsejson);

            var MOBFOPTravelerCertificateResponsejson = TestDataGenerator.GetFileContent(@"ClearCartOnBackNavigation\MOBFOPTravelerCertificateResponse.json");
            var MOBFOPTravelerCertificateResponse = JsonConvert.DeserializeObject<List<MOBFOPTravelerCertificateResponse>>(MOBFOPTravelerCertificateResponsejson);


            //var MOBBookingRegisterSeatsResponsejson = TestDataGenerator.GetFileContent(@"ClearCartOnBackNavigation\MOBApplyPromoCodeResponse.json");
            //var MOBBookingRegisterSeatsResponse = JsonConvert.DeserializeObject<List<MOBBookingRegisterOfferResponse>>(MOBBookingRegisterSeatsResponsejson);


            return new object[] { MOBClearCartOnBackNavigationRequest[0], session[0], Reservation[0], MOBShoppingCart[0], MOBSeatMapCSL30[0], ShoppingResponse[0], FlightSeatMapDetail[0], MOBBookingBundlesResponse[0], GetOffers[0], VendorOffers[0], CCEFlightReservationResponseByCartId[0], FlightReservationResponse[0], PKDispenserResponse[0], CSLShopRequest[0], MOBFutureFlightCreditResponse[0], MOBCSLContentMessagesResponse[0], MOBApplyPromoCodeResponse[0], MOBFOPTravelerCertificateResponse[0] };
        }

        public Object[] Set7()
        {
            var MOBSHOPUnfinishedBookingRequestBasejson = TestDataGenerator.GetFileContent(@"RegisterOffersForOmniCartSavedTrip\MOBSHOPUnfinishedBookingRequestBase.json");
            var MOBSHOPUnfinishedBookingRequestBase = JsonConvert.DeserializeObject<List<MOBSHOPUnfinishedBookingRequestBase>>(MOBSHOPUnfinishedBookingRequestBasejson);

            var sessionjson = TestDataGenerator.GetFileContent(@"RegisterOffersForOmniCartSavedTrip\Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);

            var Reservationjson = TestDataGenerator.GetFileContent(@"RegisterOffersForOmniCartSavedTrip\Reservation.json");
            var Reservation = JsonConvert.DeserializeObject<List<Reservation>>(Reservationjson);


            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"RegisterOffersForOmniCartSavedTrip\MOBShoppingCart.json");
            var MOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);


            var CCEFlightReservationResponseByCartIdjson = TestDataGenerator.GetFileContent(@"RegisterOffersForBooking\CCEFlightReservationResponseByCartId.json");
            var CCEFlightReservationResponseByCartId = JsonConvert.DeserializeObject<List<CCEFlightReservationResponseByCartId>>(CCEFlightReservationResponseByCartIdjson);

            var SelectSeatsjson = TestDataGenerator.GetFileContent(@"RegisterOffersForOmniCartSavedTrip\SelectSeats.json");
            var SelectSeats = JsonConvert.DeserializeObject<List<SelectSeats>>(SelectSeatsjson);


            var MOBApplyPromoCodeResponsejson = TestDataGenerator.GetFileContent(@"RegisterOffersForOmniCartSavedTrip\MOBApplyPromoCodeResponse.json");
            var MOBApplyPromoCodeResponse = JsonConvert.DeserializeObject<List<MOBApplyPromoCodeResponse>>(MOBApplyPromoCodeResponsejson);


            var MOBSeatMapCSL30json = TestDataGenerator.GetFileContent(@"RegisterOffersForOmniCartSavedTrip\MOBSeatMapCSL30.json");
            var MOBSeatMapCSL30 = JsonConvert.DeserializeObject<List<MOBSeatMapCSL30>>(MOBSeatMapCSL30json);


            var MOBFutureFlightCreditResponsejson = TestDataGenerator.GetFileContent(@"RegisterOffersForOmniCartSavedTrip\MOBFutureFlightCreditResponse.json");
            var MOBFutureFlightCreditResponse = JsonConvert.DeserializeObject<List<MOBFutureFlightCreditResponse>>(MOBFutureFlightCreditResponsejson);

            var MOBCSLContentMessagesResponsejson = TestDataGenerator.GetFileContent(@"RegisterOffersForOmniCartSavedTrip\MOBCSLContentMessagesResponse.json");
            var MOBCSLContentMessagesResponse = JsonConvert.DeserializeObject<List<MOBCSLContentMessagesResponse>>(MOBCSLContentMessagesResponsejson);


            var MOBFOPTravelerCertificateResponsejson = TestDataGenerator.GetFileContent(@"RegisterOffersForOmniCartSavedTrip\MOBFOPTravelerCertificateResponse.json");
            var MOBFOPTravelerCertificateResponse = JsonConvert.DeserializeObject<List<MOBFOPTravelerCertificateResponse>>(MOBFOPTravelerCertificateResponsejson);


            var SeatChangeStatejson = TestDataGenerator.GetFileContent(@"RegisterOffersForOmniCartSavedTrip\SeatChangeState.json");
            var SeatChangeState = JsonConvert.DeserializeObject<List<SeatChangeState>>(SeatChangeStatejson);

            var hashPinValidatejson = TestDataGenerator.GetFileContent(@"RegisterOffersForOmniCartSavedTrip\HashPinValidate.json");
            var hashPinValidate = JsonConvert.DeserializeObject<HashPinValidate>(hashPinValidatejson);

            var cCEFlightReservationResponseByCartIdsJson = TestDataGenerator.GetFileContent(@"RegisterOffersForOmniCartSavedTrip\CCEFlightReservationResponseByCartId.json");
            var cCEFlightReservationResponseByCartIds = JsonConvert.DeserializeObject<List<List<CCEFlightReservationResponseByCartId>>>(cCEFlightReservationResponseByCartIdsJson);

            var mOBBookingBundlesResponsejson = TestDataGenerator.GetFileContent(@"RegisterOffersForOmniCartSavedTrip\MOBBookingBundlesResponse.json");
            var mOBBookingBundlesResponse = JsonConvert.DeserializeObject<List<MOBBookingBundlesResponse>>(mOBBookingBundlesResponsejson);

            var mOBBookingRegisterOfferResponseJson = TestDataGenerator.GetFileContent(@"RegisterOffersForOmniCartSavedTrip\MOBBookingRegisterOfferResponse.json");
            var mOBBookingRegisterOfferResponse = JsonConvert.DeserializeObject<MOBBookingRegisterOfferResponse>(mOBBookingRegisterOfferResponseJson);

            var eligibleFormofPaymentsjson = TestDataGenerator.GetFileContent(@"RegisterOffersForOmniCartSavedTrip\EligibleFormofPayments.json");
            var eligibleFormofPayments = JsonConvert.DeserializeObject<List<List<FormofPaymentOption>>>(eligibleFormofPaymentsjson);


            return new object[] { MOBSHOPUnfinishedBookingRequestBase[0], session[0], Reservation[0], MOBShoppingCart[0], CCEFlightReservationResponseByCartId[0], SelectSeats[0], MOBApplyPromoCodeResponse[0], MOBSeatMapCSL30[0], MOBFutureFlightCreditResponse[0], MOBCSLContentMessagesResponse[0], MOBFOPTravelerCertificateResponse[0], SeatChangeState[0], hashPinValidate, cCEFlightReservationResponseByCartIds[0], mOBBookingBundlesResponse[0], mOBBookingRegisterOfferResponse, eligibleFormofPayments[0] };
        }
        public Object[] Set8()
        {
            var MOBRegisterOfferRequestjson = TestDataGenerator.GetFileContent(@"UnRegisterAncillaryOffersForBooking\MOBRegisterOfferRequest.json");
            var MOBRegisterOfferRequest = JsonConvert.DeserializeObject<List<MOBRegisterOfferRequest>>(MOBRegisterOfferRequestjson);

            var sessionjson = TestDataGenerator.GetFileContent(@"UnRegisterAncillaryOffersForBooking\Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);

            var Reservationjson = TestDataGenerator.GetFileContent(@"UnRegisterAncillaryOffersForBooking\Reservation.json");
            var Reservation = JsonConvert.DeserializeObject<List<Reservation>>(Reservationjson);


            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"UnRegisterAncillaryOffersForBooking\MOBShoppingCart.json");
            var MOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);


            var MOBApplyPromoCodeResponsejson = TestDataGenerator.GetFileContent(@"UnRegisterAncillaryOffersForBooking\MOBApplyPromoCodeResponse.json");
            var MOBApplyPromoCodeResponse = JsonConvert.DeserializeObject<List<MOBApplyPromoCodeResponse>>(MOBApplyPromoCodeResponsejson);

            var MOBCSLContentMessagesResponsejson = TestDataGenerator.GetFileContent(@"UnRegisterAncillaryOffersForBooking\MOBCSLContentMessagesResponse.json");
            var MOBCSLContentMessagesResponse = JsonConvert.DeserializeObject<List<MOBCSLContentMessagesResponse>>(MOBCSLContentMessagesResponsejson);

            var FlightReservationResponsejson = TestDataGenerator.GetFileContent(@"UnRegisterAncillaryOffersForBooking\FlightReservationResponse.json");
            var FlightReservationResponse = JsonConvert.DeserializeObject<List<FlightReservationResponse>>(FlightReservationResponsejson);

            var ProfileResponsejson = TestDataGenerator.GetFileContent(@"UnRegisterAncillaryOffersForBooking\ProfileResponse.json");
            var ProfileResponse = JsonConvert.DeserializeObject<List<ProfileResponse>>(ProfileResponsejson);

            var ProfileFOPCreditCardResponsejson = TestDataGenerator.GetFileContent(@"UnRegisterAncillaryOffersForBooking\ProfileFOPCreditCardResponse.json");
            var ProfileFOPCreditCardResponse = JsonConvert.DeserializeObject<List<ProfileFOPCreditCardResponse>>(ProfileFOPCreditCardResponsejson);

            var PKDispenserKeyjson = TestDataGenerator.GetFileContent(@"UnRegisterAncillaryOffersForBooking\PKDispenserKey.json");
            var PKDispenserKey = JsonConvert.DeserializeObject<List<PKDispenserKey>>(PKDispenserKeyjson);



            return new object[] { MOBRegisterOfferRequest[0], session[0], Reservation[0], MOBShoppingCart[0], MOBApplyPromoCodeResponse[0], MOBCSLContentMessagesResponse[0], FlightReservationResponse[0], ProfileResponse[0], ProfileFOPCreditCardResponse[0], PKDispenserKey[0] };
        }

        public Object[] Set9()
        {
            var MOBRegisterSeatsRequestjson = TestDataGenerator.GetFileContent(@"RegisterSeatsForReshop\MOBRegisterSeatsRequest1.json");
            var MOBRegisterSeatsRequest = JsonConvert.DeserializeObject<List<MOBRegisterSeatsRequest>>(MOBRegisterSeatsRequestjson);

            var sessionjson = TestDataGenerator.GetFileContent(@"RegisterSeatsForReshop\Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);

            var SelectSeatsjson = TestDataGenerator.GetFileContent(@"RegisterSeatsForReshop\SelectSeats.json");
            var SelectSeats = JsonConvert.DeserializeObject<List<SelectSeats>>(SelectSeatsjson);


            // var Reservationjson = TestDataGenerator.GetFileContent(@"RegisterSeatsForReshop\Reservation.json");
            //var Reservation = JsonConvert.DeserializeObject<List<Reservation>>(Reservationjson);

            var Reservation = TestDataGenerator.GetXmlData<Reservation>(@"RegisterSeatsForReshop\E00A45EDC7A94840B04A5A13D7440B9AUnited.Persist.Definition.Shopping.Reservation.xml");

            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"RegisterSeatsForReshop\MOBShoppingCart.json");
            var MOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);

            var SelectTripjson = TestDataGenerator.GetFileContent(@"RegisterSeatsForReshop\SelectTrip.json");
            var SelectTrip = JsonConvert.DeserializeObject<List<SelectTrip>>(SelectTripjson);


            var MOBApplyPromoCodeResponsejson = TestDataGenerator.GetFileContent(@"RegisterSeatsForReshop\MOBApplyPromoCodeResponse.json");
            var MOBApplyPromoCodeResponse = JsonConvert.DeserializeObject<List<MOBApplyPromoCodeResponse>>(MOBApplyPromoCodeResponsejson);

            var FlightReservationResponsejson = TestDataGenerator.GetFileContent(@"RegisterSeatsForReshop\FlightReservationResponse.json");
            var FlightReservationResponse = JsonConvert.DeserializeObject<List<FlightReservationResponse>>(FlightReservationResponsejson);

            var MOBFOPTravelerCertificateResponsejson = TestDataGenerator.GetFileContent(@"RegisterSeatsForReshop\MOBFOPTravelerCertificateResponse.json");
            var MOBFOPTravelerCertificateResponse = JsonConvert.DeserializeObject<MOBFOPTravelerCertificateResponse>(MOBFOPTravelerCertificateResponsejson);

            var MOBReshopRegisterSeatsResponsejson = TestDataGenerator.GetFileContent(@"RegisterSeatsForReshop\MOBReshopRegisterSeatsResponse.json");
            var MOBReshopRegisterSeatsResponse = JsonConvert.DeserializeObject<List<MOBReshopRegisterSeatsResponse>>(MOBReshopRegisterSeatsResponsejson);


            return new object[] { MOBRegisterSeatsRequest[0], session[0], SelectSeats[0], Reservation, MOBShoppingCart[0], SelectTrip[0], MOBApplyPromoCodeResponse[0], FlightReservationResponse[0], MOBFOPTravelerCertificateResponse, MOBReshopRegisterSeatsResponse[0] };
        }
        public Object[] Set10()
        {
            var MOBRegisterOfferRequestjson = TestDataGenerator.GetFileContent(@"RegisterOffersForReshop\MOBRegisterOfferRequest1.json");
            var MOBRegisterOfferRequest = JsonConvert.DeserializeObject<List<MOBRegisterOfferRequest>>(MOBRegisterOfferRequestjson);

            var sessionjson = TestDataGenerator.GetFileContent(@"RegisterOffersForReshop\Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);

            var Reservationjson = TestDataGenerator.GetFileContent(@"RegisterOffersForReshop\Reservation.json");
            var reservation = JsonConvert.DeserializeObject<List<Reservation>>(Reservationjson);



            var MOBShoppingCartjson = TestDataGenerator.GetFileContent(@"RegisterOffersForReshop\MOBShoppingCart.json");
            var mOBShoppingCart = JsonConvert.DeserializeObject<List<MOBShoppingCart>>(MOBShoppingCartjson);

            var GetOffersjson = TestDataGenerator.GetFileContent(@"RegisterOffersForReshop\GetOffers.json");
            var GetOffers = JsonConvert.DeserializeObject<List<GetOffers>>(GetOffersjson);


            var ShopBookingDetailsResponsejson = TestDataGenerator.GetFileContent(@"RegisterOffersForReshop\ShopBookingDetailsResponse.json");
            var ShopBookingDetailsResponse = JsonConvert.DeserializeObject<List<ShopBookingDetailsResponse>>(ShopBookingDetailsResponsejson);

            var FlightReservationResponsejson = TestDataGenerator.GetFileContent(@"RegisterOffersForReshop\FlightReservationResponse.json");
            var FlightReservationResponse = JsonConvert.DeserializeObject<List<FlightReservationResponse>>(FlightReservationResponsejson);

            var VendorOffersjson = TestDataGenerator.GetFileContent(@"RegisterOffersForReshop\VendorOffers.json");
            var VendorOffers = JsonConvert.DeserializeObject<List<GetVendorOffers>>(VendorOffersjson);


            var PKDispenserKeyjson = TestDataGenerator.GetFileContent(@"RegisterOffersForReshop\PKDispenserKey.json");
            var PKDispenserKeyResponse = JsonConvert.DeserializeObject<List<PKDispenserKey>>(PKDispenserKeyjson);

            var MOBFOPTravelerCertificateResponsejson = TestDataGenerator.GetFileContent(@"RegisterOffersForReshop\MOBFOPTravelerCertificateResponse.json");
            var MOBFOPTravelerCertificateResponse = JsonConvert.DeserializeObject<MOBFOPTravelerCertificateResponse>(MOBFOPTravelerCertificateResponsejson);




            var MOBSHOPReservationjson = TestDataGenerator.GetFileContent(@"RegisterOffersForReshop\MOBSHOPReservation.json");
            var MOBSHOPReservation = JsonConvert.DeserializeObject<List<MOBSHOPReservation>>(MOBSHOPReservationjson);

            //var MOBBookingRegisterOfferResponsejson = TestDataGenerator.GetFileContent(@"RegisterOffersForReshop\MOBRegisterOfferResponse.json");
            //var MOBBookingRegisterOfferResponse = JsonConvert.DeserializeObject<List<MOBBookingRegisterOfferResponse>>(MOBBookingRegisterOfferResponsejson);


            return new object[] { MOBRegisterOfferRequest[0], session[0], reservation[0], mOBShoppingCart[0], GetOffers[0], ShopBookingDetailsResponse[0], FlightReservationResponse[0], VendorOffers[0], PKDispenserKeyResponse[0], MOBFOPTravelerCertificateResponse, MOBSHOPReservation[0] };
        }
    }
}

