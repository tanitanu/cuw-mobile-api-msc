using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Definition;
using United.Definition.FormofPayment;
using United.Definition.Shopping;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Mobile.Model.Common;
using United.Persist.Definition.Shopping;
using United.Services.FlightShopping.Common.DisplayCart;
using United.Services.FlightShopping.Common.Extensions;
using United.Services.FlightShopping.Common.FlightReservation;
using United.Utility.Helper;

namespace United.Common.Helper
{
    public class MoneyPlusMilesUtility : IMoneyPlusMilesUtility
    {
        private readonly IConfiguration _configuration;
        private readonly ISessionHelperService _sessionHelperService;
        private string milesMoneyValueDesc = string.Empty;
        private readonly IShoppingCartUtility _shoppingCartUtility;
        private string milesMoneyErrorMessage = string.Empty;
        private readonly IFlightShoppingProductsService _flightShoppingProductsServices;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly ICacheLog<MoneyPlusMilesUtility> _logger;
        public MoneyPlusMilesUtility(IConfiguration configuration, ISessionHelperService sessionHelperService, IShoppingCartUtility shoppingCartUtility,
            IFlightShoppingProductsService flightShoppingProductsServices, IShoppingCartService shoppingCartService,ICacheLog<MoneyPlusMilesUtility> logger)
        {
            _configuration = configuration;
            _sessionHelperService = sessionHelperService;
            _shoppingCartUtility = shoppingCartUtility;
            _flightShoppingProductsServices = flightShoppingProductsServices;
            _shoppingCartService = shoppingCartService;
            _logger = logger;
        }

        public async Task GetMoneyPlusMilesOptionsForFinalRTIScreen(MOBRegisterSeatsRequest request, MOBBookingRegisterSeatsResponse response, United.Persist.Definition.Shopping.Session session, MOBShoppingCart shoppingCart)
        {
            // For Money Plus Miles feature on RTI there is a need to make GetMoneyPlus miles all in the review booking screen if they are eligible for M+M
            GetMoneyPlusMilesRequest moneyPlusMilesRequest = new GetMoneyPlusMilesRequest();
            MOBFOPResponse moneyPlusMilesOptions = null;
            moneyPlusMilesRequest.Application = request.Application;
            moneyPlusMilesRequest.AccessCode = request.AccessCode;
            moneyPlusMilesRequest.Flow = request.Flow;
            moneyPlusMilesRequest.LanguageCode = request.LanguageCode;
            moneyPlusMilesRequest.MileagePlusNumber = request.MileagePlusNumber;
            moneyPlusMilesRequest.SessionId = request.SessionId;
            moneyPlusMilesRequest.TransactionId = request.TransactionId;
            if (shoppingCart?.FormofPaymentDetails?.MoneyPlusMilesCredit == null)
            {
                moneyPlusMilesOptions = await GetMilesPlusMoneyOptions(session, moneyPlusMilesRequest);
                if (moneyPlusMilesOptions?.ShoppingCart?.FormofPaymentDetails?.MoneyPlusMilesCredit != null)
                    await _sessionHelperService.SaveSession(moneyPlusMilesOptions.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit, session.SessionId, new List<string> { session.SessionId, new MOBFOPMoneyPlusMilesCredit().ObjectName }, new MOBFOPMoneyPlusMilesCredit().ObjectName).ConfigureAwait(false);
                response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit = moneyPlusMilesOptions.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit;
                if (session.IsMoneyPlusMilesSelected)
                {
                    var milesOwed = response.Reservation?.Prices?.FirstOrDefault(a => a.DisplayType == "MILESANDMONEY")?.Value;
                    response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit.SelectedMoneyPlusMiles = response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit.MilesPlusMoneyOptions.FirstOrDefault(mm => mm.MilesMoneyValue == milesOwed);
                }
            }
            else
            {
                response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit = shoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit;
                if(shoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit.SelectedMoneyPlusMiles == null && session.IsMoneyPlusMilesSelected)
                    response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit.SelectedMoneyPlusMiles = shoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit.MilesPlusMoneyOptions.LastOrDefault();
                await _sessionHelperService.SaveSession(response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit, session.SessionId, new List<string> { session.SessionId, new MOBFOPMoneyPlusMilesCredit().ObjectName }, new MOBFOPMoneyPlusMilesCredit().ObjectName).ConfigureAwait(false);
            }
           

        }


        public async Task<MOBFOPResponse> GetMilesPlusMoneyOptions(Session session, GetMoneyPlusMilesRequest request)
        {
            MOBFOPResponse milesPlusMoneyCreditResponse = new MOBFOPResponse();
            try
            {
                List<CMSContentMessage> lstMessages = await _shoppingCartUtility.GetSDLContentByGroupName(request, session.SessionId, session.Token, _configuration.GetValue<string>("CMSContentMessages_GroupName_BookingRTI_Messages"), _configuration.GetValue<string>("BookingPathRTI_CMSContentMessagesCached_StaticGUID"));

                MOBShoppingCart shoppingCart = await _sessionHelperService.GetSession<MOBShoppingCart>(session.SessionId, new MOBShoppingCart().ObjectName, new List<string> { session.SessionId, new MOBShoppingCart().ObjectName }).ConfigureAwait(false);

                int totalMilesAvailable = 0;
                string mileagePlusTraveler = string.Empty;
                var cartInfo = await GetCartInformation(session.SessionId, request.Application, session.DeviceID, session.CartId, session.Token);
                if (cartInfo?.Reservation?.Sponsor?.AccountBalances?.Count > 0)
                {
                    totalMilesAvailable = cartInfo.Reservation.Sponsor.AccountBalances[0].Balance;
                    mileagePlusTraveler = $"{cartInfo.Reservation.Sponsor.GivenName} {cartInfo.Reservation.Sponsor.Surname}";
                }

                var milesAndMoneyOptions = await GetCSLMilesPlusMoneyOptions(session, request, cartInfo).ConfigureAwait(false);

                var milesAndMoneyCredit = BuildMilePlusMoneyCredit(milesAndMoneyOptions, lstMessages, totalMilesAvailable, session.IsMoneyPlusMilesSelected);

                milesAndMoneyCredit.MileagePlusTraveler = mileagePlusTraveler;
                await LoadSessionValuesToResponse(session, milesPlusMoneyCreditResponse, milesAndMoneyCredit, "", null, lstMessages);
            }
            catch
            {
                throw new MOBUnitedException(milesMoneyErrorMessage != null ? milesMoneyErrorMessage : _configuration.GetValue<string>("Booking2OGenericExceptionMessage"));
            }

            return milesPlusMoneyCreditResponse;
        }

        private async Task LoadSessionValuesToResponse(Session session, MOBFOPResponse response, MOBFOPMoneyPlusMilesCredit moneyPlusMilesCredit = null, string optionId = "", MOBShoppingCart shoppingCart = null, List<CMSContentMessage> lstMessages = null)
        {

            response.ShoppingCart = shoppingCart ?? await _sessionHelperService.GetSession<MOBShoppingCart>(session.SessionId, new MOBShoppingCart().ObjectName, new List<string> { session.SessionId, new MOBShoppingCart().ObjectName }).ConfigureAwait(false);

            if (response.ShoppingCart == null)
            {
                response.ShoppingCart = new MOBShoppingCart();
            }
            if (response.ShoppingCart?.FormofPaymentDetails == null)
            {
                response.ShoppingCart.FormofPaymentDetails = new MOBFormofPaymentDetails();
            }

            if (moneyPlusMilesCredit != null)
            {
                response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit = moneyPlusMilesCredit;
            }
            else
            {
                if (!string.IsNullOrEmpty(optionId))
                {
                    response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit.SelectedMoneyPlusMiles = response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit.MilesPlusMoneyOptions.FirstOrDefault(mm => mm.OptionId == optionId);
                    var changeInTravelerMessage = string.Empty;
                    changeInTravelerMessage = GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.RemoveCoupons")[0]?.ContentFull;
                    response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit.PromoCodeMoneyMilesAlertMessage = string.IsNullOrWhiteSpace(changeInTravelerMessage) ? null : new MOBSection
                    {
                        Text1 = changeInTravelerMessage,
                        Text2 = "Cancel",
                        Text3 = "Continue"
                    };
                }
                else
                {
                    response.ShoppingCart.FormofPaymentDetails.MoneyPlusMilesCredit = null;
                }
            }

            await _sessionHelperService.SaveSession(response.ShoppingCart, session.SessionId, new List<string> { session.SessionId, new MOBShoppingCart().ObjectName }, new MOBShoppingCart().ObjectName).ConfigureAwait(false);

            response.SessionId = session.SessionId;
            response.Flow = string.IsNullOrEmpty(session.Flow) ? response.ShoppingCart.Flow : session.Flow;
        }

        private MOBFOPMoneyPlusMilesCredit BuildMilePlusMoneyCredit(MilesAndMoneyResponse milesAndMoneyOptions, List<CMSContentMessage> lstMessages, int totalMilesAvailable, bool isMoneyPlusMilesSelectedInFSR)
        {
            var milesPlusMoneyCredit = new MOBFOPMoneyPlusMilesCredit();
            milesPlusMoneyCredit.MMCMessages = GetMMCContentMessages(lstMessages);
            milesPlusMoneyCredit.ReviewMMCMessage = GetReviewContentMessages(lstMessages);
            milesPlusMoneyCredit.MilesPlusMoneyOptions = LoadMilesPlusMoneyOptions(milesAndMoneyOptions, totalMilesAvailable);
            milesPlusMoneyCredit.TotalMilesAvailable = totalMilesAvailable.ToString("#,##0");
            if (isMoneyPlusMilesSelectedInFSR)
            {
                var changeInTravelerMessage = string.Empty;
                changeInTravelerMessage = GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.RemoveCoupons")[0]?.ContentFull;
                milesPlusMoneyCredit.PromoCodeMoneyMilesAlertMessage = string.IsNullOrWhiteSpace(changeInTravelerMessage) ? null : new MOBSection
                {
                    Text1 = changeInTravelerMessage,
                    Text2 = "Cancel",
                    Text3 = "Continue"
                };
            }
            return milesPlusMoneyCredit;
        }

        private List<MOBMobileCMSContentMessages> GetSDLMessageFromList(List<CMSContentMessage> list, string title)
        {
            List<MOBMobileCMSContentMessages> listOfMessages = new List<MOBMobileCMSContentMessages>();
            list?.Where(l => l.Title.ToUpper().Equals(title.ToUpper()))?.ForEach(i => listOfMessages.Add(new MOBMobileCMSContentMessages()
            {
                Title = i.Title,
                ContentFull = i.ContentFull,
                HeadLine = i.Headline,
                ContentShort = i.ContentShort,
                LocationCode = i.LocationCode
            }));

            return listOfMessages;
        }

        private List<MOBMobileCMSContentMessages> GetMMCContentMessages(List<CMSContentMessage> lstMessages)
        {
            List<MOBMobileCMSContentMessages> mmcMessage = new List<MOBMobileCMSContentMessages>();

            milesMoneyValueDesc = GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.MilesMoneyValue").FirstOrDefault()?.ContentFull; // Assigning MilesMoneyValue generic description string ${0} off on this trip
            mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.Information"));
            mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.BtnText"));
            mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.SelectMiles"));
            mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.MilesApplied"));
            mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.MoneyBalanceDue"));
            mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.AvailableMiles"));
            mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.MilesRemaining"));
            mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.TotalPrice"));
            mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.AvailableMilesTrip"));
            if (!_configuration.GetValue<bool>("isEnableDueInformation"))
            {
                mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.DueInformation1"));
                mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.DueInformation2"));
                mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.MMCMessage.DueInformation"));
            }
            var errorMsg = GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.ReviewMMCMessage.ErrorMsg");
            milesMoneyErrorMessage = errorMsg.FirstOrDefault()?.ContentShort;
            mmcMessage.AddRange(errorMsg);
            mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.AboutMMCMessage.AboutMoneyPlusMiles"));
            mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.AboutMMCMessage.TransactionFailed"));
            mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.AboutMMCMessage.MoneyPlusMilesMsg"));
            mmcMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.AboutMMCMessage.BtnBackToMoneyPlusMiles"));

            return mmcMessage;
        }

        private List<MOBMobileCMSContentMessages> GetReviewContentMessages(List<CMSContentMessage> lstMessages)
        {
            List<MOBMobileCMSContentMessages> reviewMCMMessage = new List<MOBMobileCMSContentMessages>();

            reviewMCMMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.ReviewMMCMessage.Includestaxandfees"));
            reviewMCMMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.ReviewMMCMessage.PriceBreakdown"));
            reviewMCMMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.ReviewMMCMessage.MileagePlusTraveler"));
            reviewMCMMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.ReviewMMCMessage.AvailableMiles"));
            reviewMCMMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.ReviewMMCMessage.RemainingMiles"));
            reviewMCMMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.ReviewMMCMessage.Fare"));
            reviewMCMMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.ReviewMMCMessage.MilesApplied"));
            reviewMCMMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.ReviewMMCMessage.MoneyBalanceDue"));
            reviewMCMMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.ReviewMMCMessage.ErrorMsg"));
            reviewMCMMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.AboutMMCMessage.TransactionFailed"));
            reviewMCMMessage.AddRange(GetSDLMessageFromList(lstMessages, "RTI.MoneyPlusMilesCredits.ReviewMMCMessage.BtnRemoveMoneyPlusMiles"));

            return reviewMCMMessage;
        }
        private List<MOBFOPMoneyPlusMiles> LoadMilesPlusMoneyOptions(MilesAndMoneyResponse milesAndMoneyOptions, int totalMilesAvailable)
        {
            List<MOBFOPMoneyPlusMiles> milesMoneyOptions = new List<MOBFOPMoneyPlusMiles>();
            if (milesAndMoneyOptions?.MilesAndMoneyOptions != null && milesAndMoneyOptions?.MilesAndMoneyOptions.Count > 0)
            {
                foreach (var milesPlusMoneyOption in milesAndMoneyOptions.MilesAndMoneyOptions)
                {
                    if (milesPlusMoneyOption != null)
                    {
                        var milesMoneyOption = new MOBFOPMoneyPlusMiles
                        {
                            OptionId = milesPlusMoneyOption.OptionId,
                            ConversionRate = milesPlusMoneyOption.ConversionRate,
                            MilesMoneyValue = milesPlusMoneyOption.MilesMoneyValue,
                            MilesOwed = milesPlusMoneyOption.MilesOwed,
                            MilesPercentage = milesPlusMoneyOption.MilesPercentage,
                            MoneyOwed = milesPlusMoneyOption.MoneyOwed,
                            MilesApplied = milesPlusMoneyOption.MilesOwed.ToString("#,##0"),
                            MoneyDiscountForMiles = milesMoneyValueDesc != string.Empty ? string.Format(milesMoneyValueDesc, milesPlusMoneyOption.MilesMoneyValue.ToString("#,##0.00")) : milesPlusMoneyOption.MilesMoneyValue.ToString("#,##0.00"),
                            MilesRemaining = (totalMilesAvailable - milesPlusMoneyOption.MilesOwed).ToString("#,##0"),
                            MoneyBalanceDue = "$" + milesPlusMoneyOption.MoneyOwed.ToString("#,##0.00"),
                            ReviewMilesApplied = "-$" + milesPlusMoneyOption.MilesMoneyValue.ToString("#,##0.00"),
                            Fare = "$" + (milesPlusMoneyOption.MilesMoneyValue + milesPlusMoneyOption.MoneyOwed).ToString("#,##0.00")
                        };
                        milesMoneyOptions.Add(milesMoneyOption);
                    }
                }
            }
            else
            {
                throw new System.Exception("No options returned in GetMilesAndMoneyOptions CSL service call");
            }

            return milesMoneyOptions;
        }

        private async Task<LoadReservationAndDisplayCartResponse> GetCartInformation(
           string sessionId, MOBApplication application, string deviceid, string cartId, string token,
           WorkFlowType workFlowType = WorkFlowType.InitialBooking)
        {
            LoadReservationAndDisplayCartRequest loadReservationAndDisplayCartRequest = new LoadReservationAndDisplayCartRequest();
            LoadReservationAndDisplayCartResponse loadReservationAndDisplayResponse = new LoadReservationAndDisplayCartResponse();
            loadReservationAndDisplayCartRequest.CartId = cartId;
            loadReservationAndDisplayCartRequest.WorkFlowType = workFlowType;
            string jsonRequest = Newtonsoft.Json.JsonConvert.SerializeObject(loadReservationAndDisplayCartRequest);

            string actionName = "LoadReservationAndDisplayCart";

            loadReservationAndDisplayResponse = await _shoppingCartService.GetCartInformation<LoadReservationAndDisplayCartResponse>(token, actionName, jsonRequest, sessionId).ConfigureAwait(false);

            //loadReservationAndDisplayResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<LoadReservationAndDisplayCartResponse>(jsonResponse);
            return loadReservationAndDisplayResponse;
        }

        private async Task<MilesAndMoneyResponse> GetCSLMilesPlusMoneyOptions(Session session, MOBRequest mobRequest, LoadReservationAndDisplayCartResponse cartInfo)
        {
            MilesAndMoneyResponse response = new MilesAndMoneyResponse();
            string url = "GetMilesAndMoneyOptions";

            MilesAndMoneyRequest cslRequest = new MilesAndMoneyRequest();
            try
            {
                cslRequest.CartId = session.CartId;
                cslRequest.Reservation = cartInfo.Reservation;
                cslRequest.Characteristics = new System.Collections.ObjectModel.Collection<Service.Presentation.CommonModel.Characteristic>() { new Service.Presentation.CommonModel.Characteristic() { Code = "LOADFROMSESSION", Value = "True" } };
                string jsonRequest = JsonConvert.SerializeObject(cslRequest);
                string cslresponse = await _flightShoppingProductsServices.GetCSLMilesPlusMoneyOptions(session.Token, url, jsonRequest, session.SessionId).ConfigureAwait(false);
                response = JsonConvert.DeserializeObject<MilesAndMoneyResponse>(cslresponse);
            }
            catch
            {
                _logger.ILoggerWarning("GetCSLMilesPlusMoneyOptions Request {@Request} : Deserialized Response {@Response}", cslRequest , response);
            }
            return response;
        }

        private static GetMoneyPlusMilesRequest CreateGetMoneyPlusMilesRequest(MOBRegisterSeatsRequest request)
        {
            GetMoneyPlusMilesRequest moneyPlusMilesRequest = new GetMoneyPlusMilesRequest();
            moneyPlusMilesRequest.Application = request.Application;
            moneyPlusMilesRequest.AccessCode = request.AccessCode;
            moneyPlusMilesRequest.Flow = request.Flow;
            moneyPlusMilesRequest.LanguageCode = request.LanguageCode;
            moneyPlusMilesRequest.MileagePlusNumber = request.MileagePlusNumber;
            moneyPlusMilesRequest.SessionId = request.SessionId;
            moneyPlusMilesRequest.TransactionId = request.TransactionId;
            return moneyPlusMilesRequest;
        }
    }
}
