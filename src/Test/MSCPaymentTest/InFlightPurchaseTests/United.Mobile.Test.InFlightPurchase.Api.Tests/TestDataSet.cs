using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using United.Definition.FormofPayment;
//using United.Definition.FormofPayment.MOBInFlightPurchaseResponse;
//using  United.Definition.FormofPayment.MOBInFlightPurchaseRequest;
using United.Definition.SDL;
using United.Mobile.Model.Common;
using United.Service.Presentation.ProductResponseModel;
//using United.Mobile.Model.InFlightPurchase;

namespace United.Mobile.Test.InFlightPurchase.Api.Tests
{
    public class TestDataSet
    {
        public Object[] set1()
        {
            var mOBInFlightPurchaseRequestjson = TestDataGenerator.GetFileContent(@"MOBInFlightPurchaseRequest.json");
            var mOBInFlightPurchaseRequest = JsonConvert.DeserializeObject<List<Definition.FormofPayment.MOBInFlightPurchaseRequest>>(mOBInFlightPurchaseRequestjson);

            var sDLKeyValuePairContentResponsejson = TestDataGenerator.GetFileContent(@"SDLKeyValuePairContentResponse.json");
            var sDLKeyValuePairContentResponse = JsonConvert.DeserializeObject<List<SDLKeyValuePairContentResponse>>(sDLKeyValuePairContentResponsejson);







            return new object[] { mOBInFlightPurchaseRequest[0], sDLKeyValuePairContentResponse[0] };
        }


        public Object[] set2()
        {
            var mOBSavedCCInflightPurchaseRequestjson = TestDataGenerator.GetFileContent(@"MOBSavedCCInflightPurchaseRequest.json");
            var mOBSavedCCInflightPurchaseRequest = JsonConvert.DeserializeObject<List<MOBSavedCCInflightPurchaseRequest>>(mOBSavedCCInflightPurchaseRequestjson);

            return new object[] { mOBSavedCCInflightPurchaseRequest[0] };
        }

        public Object[] set3()
        {
            var mOBSavedCCInflightPurchaseRequestjson = TestDataGenerator.GetFileContent(@"MOBSavedCCInflightPurchaseRequest.json");
            var mOBSavedCCInflightPurchaseRequest = JsonConvert.DeserializeObject<List<MOBSavedCCInflightPurchaseRequest>>(mOBSavedCCInflightPurchaseRequestjson);

            var mOBInFlightPurchaseResponsejson = TestDataGenerator.GetFileContent(@"MOBInFlightPurchaseResponse.json");
            var mOBInFlightPurchaseResponse  = JsonConvert.DeserializeObject<List<MOBInFlightPurchaseResponse>>(mOBInFlightPurchaseResponsejson);


            return new object[] { mOBSavedCCInflightPurchaseRequest[0], mOBInFlightPurchaseResponse[0] };
        }
    }
}
