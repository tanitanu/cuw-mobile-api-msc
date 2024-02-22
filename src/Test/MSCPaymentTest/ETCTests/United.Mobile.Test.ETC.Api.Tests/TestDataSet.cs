using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using United.Definition;
using United.Definition.FormofPayment;
using United.Definition.Shopping;
using United.Persist.Definition.Shopping;
using United.Service.Presentation.CommonModel;

namespace United.Mobile.Test.ETC.Api.Tests
{
    public class TestDataSet
    {
        public Object[] set1()
        {
            var MOBFOPTravelerCertificateRequestjson = TestDataGenarator.GetFileContent("MOBFOPTravelerCertificateRequest.json");
            var mOBFOPTravelerCertificateRequest = JsonConvert.DeserializeObject<List<MOBFOPTravelerCertificateRequest>>(MOBFOPTravelerCertificateRequestjson);

            var sessionjson = TestDataGenarator.GetFileContent(@"Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);

            var mOBFOPTravelerCertificateResponsejson = TestDataGenarator.GetFileContent("MOBFOPTravelerCertificateResponse2.json");
            var mOBFOPTravelerCertificateResponse = JsonConvert.DeserializeObject<List<MOBFOPTravelerCertificateResponse>>(mOBFOPTravelerCertificateResponsejson);

            var reservationjson = TestDataGenarator.GetFileContent("Reservation.json");
            var reservation = JsonConvert.DeserializeObject<List<Reservation>>(reservationjson);

            var MOBFOPTravelCertificatejson = TestDataGenarator.GetFileContent("MOBFOPTravelCertificate.json");
            var mOBFOPTravelCertificate = JsonConvert.DeserializeObject<List<MOBFOPTravelCertificate>>(MOBFOPTravelCertificatejson);

            var MOBSHOPPricejson = TestDataGenarator.GetFileContent("MOBSHOPPrice.json");
            var mOBSHOPPrice = JsonConvert.DeserializeObject<MOBSHOPPrice>(MOBSHOPPricejson);

            return new object[] { mOBFOPTravelerCertificateRequest[0], session[0], mOBFOPTravelerCertificateResponse[0], reservation[0], mOBFOPTravelCertificate[0], mOBSHOPPrice };
        }

        public Object[] set2()
        {
            var fOPBillingContactInfoRequestjson = TestDataGenarator.GetFileContent(@"FOPBillingContactInfoRequest.json");
            var fOPBillingContactInfoRequest = JsonConvert.DeserializeObject<List<MOBFOPBillingContactInfoRequest>>(fOPBillingContactInfoRequestjson);

            var sessionjson = TestDataGenarator.GetFileContent(@"Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);

            var mOBFOPTravelerCertificateResponsejson = TestDataGenarator.GetFileContent(@"MOBFOPTravelerCertificateResponse2.json");
            var mOBFOPTravelerCertificateResponse = JsonConvert.DeserializeObject<List<MOBFOPTravelerCertificateResponse>>(mOBFOPTravelerCertificateResponsejson);

            var stateProvincejson = TestDataGenarator.GetFileContent(@"StateProvince.json");
            var stateProvince = JsonConvert.DeserializeObject<List<StateProvince>>(stateProvincejson);

            var reservationjson = TestDataGenarator.GetFileContent(@"Reservation.json");
            var reservation = JsonConvert.DeserializeObject<List<Reservation>>(reservationjson);

            var MOBcSLContentMessagesResponsejson = TestDataGenarator.GetFileContent(@"MOBCSLContentMessagesResponse1.json");
            var mOBcSLContentMessagesResponse = JsonConvert.DeserializeObject<MOBCSLContentMessagesResponse>(MOBcSLContentMessagesResponsejson);

            var formsOfPaymentjson = TestDataGenarator.GetFileContent(@"formsOfPayment.json");
            var formsOfPayment = JsonConvert.DeserializeObject<List<FormofPaymentOption>>(formsOfPaymentjson);
            return new object[] { fOPBillingContactInfoRequest[0], session[0], mOBFOPTravelerCertificateResponse[0], stateProvince, reservation[0], true, mOBcSLContentMessagesResponse, formsOfPayment };
        }
        public Object[] set3()
        {
            var fOPBillingContactInfoRequestjson = TestDataGenarator.GetFileContent(@"FOPBillingContactInfoRequest.json");
            var fOPBillingContactInfoRequest = JsonConvert.DeserializeObject<List<MOBFOPBillingContactInfoRequest>>(fOPBillingContactInfoRequestjson);

            var sessionjson = TestDataGenarator.GetFileContent(@"Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);

            var mOBFOPTravelerCertificateResponsejson = TestDataGenarator.GetFileContent(@"MOBFOPTravelerCertificateResponse2.json");
            var mOBFOPTravelerCertificateResponse = JsonConvert.DeserializeObject<List<MOBFOPTravelerCertificateResponse>>(mOBFOPTravelerCertificateResponsejson);

            var stateProvincejson1 = TestDataGenarator.GetFileContent(@"StateProvince1.json");
            var stateProvince1 = JsonConvert.DeserializeObject<List<StateProvince>>(stateProvincejson1);

            var reservationjson = TestDataGenarator.GetFileContent(@"Reservation.json");
            var reservation = JsonConvert.DeserializeObject<List<Reservation>>(reservationjson);

            var MOBcSLContentMessagesResponsejson1 = TestDataGenarator.GetFileContent(@"MOBCSLContentMessagesResponse1.json");
            var mOBcSLContentMessagesResponse1 = JsonConvert.DeserializeObject<MOBCSLContentMessagesResponse>(MOBcSLContentMessagesResponsejson1);

            var formsOfPaymentjson = TestDataGenarator.GetFileContent(@"formsOfPayment.json");
            var formsOfPayment = JsonConvert.DeserializeObject<List<FormofPaymentOption>>(formsOfPaymentjson);

            return new object[] { fOPBillingContactInfoRequest[0], session[0], mOBFOPTravelerCertificateResponse[0], stateProvince1, reservation[0], true, mOBcSLContentMessagesResponse1, formsOfPayment };
        }

        public Object[] set4()
        {
            var MOBFOPTravelerCertificateRequestjson = TestDataGenarator.GetFileContent("MOBFOPTravelerCertificateRequest2.json");
            var mOBFOPTravelerCertificateRequest = JsonConvert.DeserializeObject<List<MOBFOPTravelerCertificateRequest>>(MOBFOPTravelerCertificateRequestjson);

            var sessionjson = TestDataGenarator.GetFileContent(@"Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);

            var mOBFOPTravelerCertificateResponsejson = TestDataGenarator.GetFileContent("MOBFOPTravelerCertificateResponse2.json");
            var mOBFOPTravelerCertificateResponse = JsonConvert.DeserializeObject<List<MOBFOPTravelerCertificateResponse>>(mOBFOPTravelerCertificateResponsejson);

            var reservationjson = TestDataGenarator.GetFileContent("Reservation.json");
            var reservation = JsonConvert.DeserializeObject<List<Reservation>>(reservationjson);

            var MOBFOPTravelCertificatejson = TestDataGenarator.GetFileContent("MOBFOPTravelCertificate.json");
            var mOBFOPTravelCertificate = JsonConvert.DeserializeObject<List<MOBFOPTravelCertificate>>(MOBFOPTravelCertificatejson);

            var MOBSHOPPricejson = TestDataGenarator.GetFileContent("MOBSHOPPrice.json");
            var mOBSHOPPrice = JsonConvert.DeserializeObject<MOBSHOPPrice>(MOBSHOPPricejson);

            return new object[] { mOBFOPTravelerCertificateRequest[0], session[0], mOBFOPTravelerCertificateResponse[0], reservation[0], mOBFOPTravelCertificate[0], mOBSHOPPrice };
        }

        public Object[] set5()
        {
            var MOBFOPTravelerCertificateRequestjson = TestDataGenarator.GetFileContent("MOBFOPTravelerCertificateRequest3.json");
            var mOBFOPTravelerCertificateRequest = JsonConvert.DeserializeObject<List<MOBFOPTravelerCertificateRequest>>(MOBFOPTravelerCertificateRequestjson);

            var sessionjson = TestDataGenarator.GetFileContent(@"Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);

            var mOBFOPTravelerCertificateResponsejson = TestDataGenarator.GetFileContent("MOBFOPTravelerCertificateResponse2.json");
            var mOBFOPTravelerCertificateResponse = JsonConvert.DeserializeObject<List<MOBFOPTravelerCertificateResponse>>(mOBFOPTravelerCertificateResponsejson);

            var reservationjson = TestDataGenarator.GetFileContent("Reservation.json");
            var reservation = JsonConvert.DeserializeObject<List<Reservation>>(reservationjson);

            var MOBFOPTravelCertificatejson = TestDataGenarator.GetFileContent("MOBFOPTravelCertificate.json");
            var mOBFOPTravelCertificate = JsonConvert.DeserializeObject<List<MOBFOPTravelCertificate>>(MOBFOPTravelCertificatejson);

            var MOBSHOPPricejson = TestDataGenarator.GetFileContent("MOBSHOPPrice.json");
            var mOBSHOPPrice = JsonConvert.DeserializeObject<MOBSHOPPrice>(MOBSHOPPricejson);

            return new object[] { mOBFOPTravelerCertificateRequest[0], session[0], mOBFOPTravelerCertificateResponse[0], reservation[0], mOBFOPTravelCertificate[0], mOBSHOPPrice };
        }

        public Object[] set6()
        {
            var MOBFOPTravelerCertificateRequestjson = TestDataGenarator.GetFileContent("MOBFOPTravelerCertificateRequest.json");
            var mOBFOPTravelerCertificateRequest = JsonConvert.DeserializeObject<List<MOBFOPTravelerCertificateRequest>>(MOBFOPTravelerCertificateRequestjson);

            var sessionjson = TestDataGenarator.GetFileContent(@"Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);

            var mOBFOPTravelerCertificateResponsejson = TestDataGenarator.GetFileContent("MOBFOPTravelerCertificateResponse2.json");
            var mOBFOPTravelerCertificateResponse = JsonConvert.DeserializeObject<List<MOBFOPTravelerCertificateResponse>>(mOBFOPTravelerCertificateResponsejson);

            var reservationjson = TestDataGenarator.GetFileContent("Reservation.json");
            var reservation = JsonConvert.DeserializeObject<List<Reservation>>(reservationjson);

            var MOBFOPTravelCertificatejson = TestDataGenarator.GetFileContent("MOBFOPTravelCertificate.json");
            var mOBFOPTravelCertificate = JsonConvert.DeserializeObject<List<MOBFOPTravelCertificate>>(MOBFOPTravelCertificatejson);

            var MOBSHOPPricejson = TestDataGenarator.GetFileContent("MOBSHOPPrice.json");
            var mOBSHOPPrice = JsonConvert.DeserializeObject<MOBSHOPPrice>(MOBSHOPPricejson);

            return new object[] { mOBFOPTravelerCertificateRequest[0], session[0], mOBFOPTravelerCertificateResponse[0], reservation[0], mOBFOPTravelCertificate[0], mOBSHOPPrice };
        }

        public Object[] set2_1()
        {
            var fOPBillingContactInfoRequestjson = TestDataGenarator.GetFileContent(@"FOPBillingContactInfoRequest1.json");
            var fOPBillingContactInfoRequest = JsonConvert.DeserializeObject<List<MOBFOPBillingContactInfoRequest>>(fOPBillingContactInfoRequestjson);

            var sessionjson = TestDataGenarator.GetFileContent(@"Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);

            var mOBFOPTravelerCertificateResponsejson = TestDataGenarator.GetFileContent(@"MOBFOPTravelerCertificateResponse3.json");
            var mOBFOPTravelerCertificateResponse = JsonConvert.DeserializeObject<List<MOBFOPTravelerCertificateResponse>>(mOBFOPTravelerCertificateResponsejson);

            var stateProvincejson = TestDataGenarator.GetFileContent(@"StateProvince.json");
            var stateProvince = JsonConvert.DeserializeObject<List<StateProvince>>(stateProvincejson);

            var reservationjson = TestDataGenarator.GetFileContent(@"Reservation.json");
            var reservation = JsonConvert.DeserializeObject<List<Reservation>>(reservationjson);

            var MOBcSLContentMessagesResponsejson = TestDataGenarator.GetFileContent(@"MOBCSLContentMessagesResponse1.json");
            var mOBcSLContentMessagesResponse = JsonConvert.DeserializeObject<MOBCSLContentMessagesResponse>(MOBcSLContentMessagesResponsejson);

            var formsOfPaymentjson = TestDataGenarator.GetFileContent(@"formsOfPayment.json");
            var formsOfPayment = JsonConvert.DeserializeObject<List<FormofPaymentOption>>(formsOfPaymentjson);
            return new object[] { fOPBillingContactInfoRequest[0], session[0], mOBFOPTravelerCertificateResponse[0], stateProvince, reservation[0], true, mOBcSLContentMessagesResponse, formsOfPayment };
        }

        public Object[] set1_1()
        {
            var MOBFOPTravelerCertificateRequestjson = TestDataGenarator.GetFileContent("MOBFOPTravelerCertificateRequest1.json");
            var mOBFOPTravelerCertificateRequest = JsonConvert.DeserializeObject<List<MOBFOPTravelerCertificateRequest>>(MOBFOPTravelerCertificateRequestjson);

            var sessionjson = TestDataGenarator.GetFileContent(@"Session.json");
            var session = JsonConvert.DeserializeObject<List<Session>>(sessionjson);

            var mOBFOPTravelerCertificateResponsejson = TestDataGenarator.GetFileContent("MOBFOPTravelerCertificateResponse1.json");
            var mOBFOPTravelerCertificateResponse = JsonConvert.DeserializeObject<List<MOBFOPTravelerCertificateResponse>>(mOBFOPTravelerCertificateResponsejson);

            var reservationjson = TestDataGenarator.GetFileContent("Reservation.json");
            var reservation = JsonConvert.DeserializeObject<List<Reservation>>(reservationjson);

            var MOBFOPTravelCertificatejson = TestDataGenarator.GetFileContent("MOBFOPTravelCertificate.json");
            var mOBFOPTravelCertificate = JsonConvert.DeserializeObject<List<MOBFOPTravelCertificate>>(MOBFOPTravelCertificatejson);

            var MOBSHOPPricejson = TestDataGenarator.GetFileContent("MOBSHOPPrice.json");
            var mOBSHOPPrice = JsonConvert.DeserializeObject<MOBSHOPPrice>(MOBSHOPPricejson);

            return new object[] { mOBFOPTravelerCertificateRequest[0], session[0], mOBFOPTravelerCertificateResponse[0], reservation[0], mOBFOPTravelCertificate[0], mOBSHOPPrice };
        }
    }
}

