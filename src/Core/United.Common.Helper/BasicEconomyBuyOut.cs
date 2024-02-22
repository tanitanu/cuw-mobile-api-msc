using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using United.Definition;
using United.Definition.CCE;
using United.Definition.Shopping;
using United.Mobile.DataAccess.Common;
using United.Mobile.Model.MSC;
using United.Service.Presentation.PersonalizationResponseModel;

namespace United.Common.Helper
{
    public class BasicEconomyBuyOut
    {
        private const string BE_BUYOUT_PRODUCT_CODE = "BEB";
        private const string IBE_BUYOUT_SDL_PRODUCT_CODE = "IBE";
        private const string PBE_BUYOUT_SDL_PRODUCT_CODE = "PBE";
        private const string CBE_BUYOUT_SDL_PRODUCT_CODE = "CBE";
        private readonly DynamicOfferDetailResponse _offerResponse;
        private readonly SDLProduct _sdlContent;
        private readonly int _noOfTravelers;
        private readonly IConfiguration _configuration;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly bool _isEnableIBEBuyOut;
        public MOBBasicEconomyBuyOut BasicEconomyBuyOutOffer { get; private set; }

        public BasicEconomyBuyOut(string sessionId, IConfiguration configuration, ISessionHelperService sessionHelperService, bool isEnableIBEBuyOut)
        {
            _configuration = configuration;
            var productOfferCce = new GetOffersCce();
            productOfferCce = sessionHelperService.GetSession<GetOffersCce>(sessionId, productOfferCce.ObjectName, new List<string> { sessionId, productOfferCce.ObjectName }).Result;

            _isEnableIBEBuyOut = isEnableIBEBuyOut;

            this._offerResponse = string.IsNullOrEmpty(productOfferCce?.OfferResponseJson)
                                    ? null
                                    : JsonConvert.DeserializeObject<DynamicOfferDetailResponse>(productOfferCce?.OfferResponseJson);
            this._sdlContent = GetSdlContent(_offerResponse);
            this._noOfTravelers = _offerResponse?.Travelers?.Count ?? 0;
        }

        public BasicEconomyBuyOut(DynamicOfferDetailResponse offerResponse, IConfiguration configuration)
        {
            this._offerResponse = offerResponse;
            this._sdlContent = GetSdlContent(offerResponse);
            this._noOfTravelers = offerResponse?.Travelers?.Count ?? 0;
            _configuration = configuration;
        }

        private SDLProduct GetSdlContent(DynamicOfferDetailResponse offerResponse)
        {
            if (offerResponse?.ResponseData == null)
                return null;

            SDLContentResponseData sdlData = offerResponse.ResponseData.ToObject<SDLContentResponseData>();
            return sdlData?.Results
                          ?.FirstOrDefault(r => (r?.Code == BE_BUYOUT_PRODUCT_CODE || r?.Code == IBE_BUYOUT_SDL_PRODUCT_CODE || r?.Code == PBE_BUYOUT_SDL_PRODUCT_CODE || r?.Code == CBE_BUYOUT_SDL_PRODUCT_CODE))
                          ?.Products
                          ?.FirstOrDefault();
        }

        /// <summary>
        /// This method extracts Terms And Conditions for BE-BuyOut from SDL Content response
        /// </summary>
        /// <returns>MOBMobileCMSContentMessages T&C's for BE-BuyOut</returns>
        public MOBMobileCMSContentMessages GetTermsAndConditionsForBEB()
        {
            if (_offerResponse?.ResponseData == null)
                return null;

            SDLContentResponseData sdlData = _offerResponse.ResponseData.ToObject<SDLContentResponseData>();
            string code = BEBCode(sdlData?.Body?.FirstOrDefault()?.name);
            string name = BEBTermsAndConditionsName(sdlData?.Body?.FirstOrDefault()?.name);

            var termsAndConditions = sdlData?.Body
                                            ?.FirstOrDefault(b => b?.name?.Equals(code, StringComparison.OrdinalIgnoreCase) ?? false)
                                            ?.content
                                            ?.FirstOrDefault(c => c?.name?.Equals(name, StringComparison.OrdinalIgnoreCase) ?? false)
                                            ?.content;

            if (termsAndConditions == null || string.IsNullOrEmpty(termsAndConditions?.body) || string.IsNullOrEmpty(termsAndConditions?.title) || string.IsNullOrEmpty(termsAndConditions?.subtitle))
                return null;

            return new MOBMobileCMSContentMessages
            {
                Title = $"{termsAndConditions?.title}",
                ContentShort = _configuration.GetValue<string>("PaymentTnCMessage"),
                HeadLine = $"{termsAndConditions?.subtitle}",
                ContentFull = $"{termsAndConditions?.body}"
            };
        }

        private string BEBCode(string code)
        {
            if (string.IsNullOrEmpty(code))
                return BE_BUYOUT_PRODUCT_CODE;

            if (_isEnableIBEBuyOut && code.Equals(IBE_BUYOUT_SDL_PRODUCT_CODE, StringComparison.OrdinalIgnoreCase))
            {
                return IBE_BUYOUT_SDL_PRODUCT_CODE;
            }
            else if (_isEnableIBEBuyOut && code.Equals(PBE_BUYOUT_SDL_PRODUCT_CODE, StringComparison.OrdinalIgnoreCase))
            {
                return PBE_BUYOUT_SDL_PRODUCT_CODE;
            }
            else if (_isEnableIBEBuyOut && code.Equals(CBE_BUYOUT_SDL_PRODUCT_CODE, StringComparison.OrdinalIgnoreCase))
            {
                return CBE_BUYOUT_SDL_PRODUCT_CODE;
            }
            else
                return BE_BUYOUT_PRODUCT_CODE;
        }
        private string BEBTermsAndConditionsName(string code)
        {
            if (string.IsNullOrEmpty(code))
                return "beb-Terms-And-Conditions";

            if (_isEnableIBEBuyOut && code.Equals(IBE_BUYOUT_SDL_PRODUCT_CODE, StringComparison.OrdinalIgnoreCase))
            {
                return IBE_BUYOUT_SDL_PRODUCT_CODE + "-TC-and-FAQ-List";
            }
            else if (_isEnableIBEBuyOut && code.Equals(PBE_BUYOUT_SDL_PRODUCT_CODE, StringComparison.OrdinalIgnoreCase))
            {
                return PBE_BUYOUT_SDL_PRODUCT_CODE + "-TC-and-FAQ-List";
            }
            else if (_isEnableIBEBuyOut && code.Equals(CBE_BUYOUT_SDL_PRODUCT_CODE, StringComparison.OrdinalIgnoreCase))
            {
                return CBE_BUYOUT_SDL_PRODUCT_CODE + "-TC-and-FAQ-List";
            }
            else
                return "beb-Terms-And-Conditions";
        }
    }
}
