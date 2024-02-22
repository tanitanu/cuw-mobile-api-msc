using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Definition;
using United.Definition.FormofPayment;
using United.Definition.TouchlessPayments;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.DynamoDB;
using United.Mobile.DataAccess.MSCPayment.Interfaces;
using United.Mobile.Model.Common;
using United.Mobile.Model.MSC;
using United.Persist.Definition.InflightPurchase;
using United.Service.Presentation.CommonModel;
using United.Service.Presentation.ProductModel;
using United.Service.Presentation.ProductResponseModel;
using United.Services.Customer.Common;
using United.Services.Customer.Profile.Common;
using United.Services.FlightShopping.Common.Extensions;
using United.Utility.Helper;
using Address = United.Definition.TouchlessPayments.Address;
using Characteristic = United.Service.Presentation.CommonModel.Characteristic;
using CreditCard = United.Definition.TouchlessPayments.CreditCard;
using CslDataVaultRequest = United.Service.Presentation.PaymentRequestModel.DataVault<United.Service.Presentation.PaymentModel.Payment>;
using CslDataVaultResponse = United.Service.Presentation.PaymentResponseModel.DataVault<United.Service.Presentation.PaymentModel.Payment>;
using Task = System.Threading.Tasks.Task;

namespace United.Mobile.InFlightPurchase.Domain
{
    public class InFlightPurchaseBusiness : IInFlightPurchaseBusiness
    {
        private readonly ICacheLog<InFlightPurchaseBusiness> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHeaders _headers;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly IReferencedataService _referencedataService;
        private readonly IDataVaultService _dataVaultService;
        private readonly IDPService _dPService;
        private readonly IPurchaseMerchandizingService _purchaseMerchandizingService;
        private readonly IGetTravelersService _getTravelersService;
        private readonly IGetMPNumberService _getMPNumberService;
        private readonly IGetSDLKeyValuePairContentService _getSDLKeyValuePairContentService;
        private readonly IDynamoDBService _dynamoDBService;
        private readonly InflightDynamoDB _inflightDynamoDB;
        public InFlightPurchaseBusiness(ICacheLog<InFlightPurchaseBusiness> logger
           , IConfiguration configuration
           , IHeaders headers
           , ISessionHelperService sessionHelperService
           , IReferencedataService referencedataService
           , IDPService dPService
           , IDataVaultService dataVaultService
           , IPurchaseMerchandizingService purchaseMerchandizingService
           , IGetTravelersService getTravelersService
           , IGetMPNumberService getMPNumberService
           , IGetSDLKeyValuePairContentService getSDLKeyValuePairContentService
            , IDynamoDBService dynamoDBService
            )
        {
            _logger = logger;
            _configuration = configuration;
            _headers = headers;
            _sessionHelperService = sessionHelperService;
            _referencedataService = referencedataService;
            _dPService = dPService;
            _dataVaultService = dataVaultService;
            _purchaseMerchandizingService = purchaseMerchandizingService;
            _getTravelersService = getTravelersService;
            _getMPNumberService = getMPNumberService;
            _getSDLKeyValuePairContentService = getSDLKeyValuePairContentService;
            _dynamoDBService = dynamoDBService;
            _inflightDynamoDB = new InflightDynamoDB(_configuration, _dynamoDBService);
        }

        public async Task<MOBInFlightPurchaseResponse> EiligibilityCheckToAddNewCCForInFlightPurchase(MOBInFlightPurchaseRequest request)
        {

            var response = new MOBInFlightPurchaseResponse();
            response = await EligibilityCheckForContactlessPaymentInFlightPurchase(request);
            await GetCLPBillingAddressCountries(response);

            return await Task.FromResult(response);
        }

        public async Task<MOBInFlightPurchaseResponse> SaveCCPNROnlyForInflightPurchase(MOBSavedCCInflightPurchaseRequest request)
        {

            var response = new MOBInFlightPurchaseResponse();
            response = await AddOrReplaceCCForInflightPurchase(request);

            return await Task.FromResult(response);
        }

        public async Task<MOBInFlightPurchaseResponse> VerifySavedCCForInflightPurchase(MOBSavedCCInflightPurchaseRequest request)
        {

            var response = new MOBInFlightPurchaseResponse();
            ContactLessPaymentState state;
            if (!Enum.TryParse(request.State, true, out state))
                throw new System.Exception($"Invalid state: {request.State}");

            PopulateMOBInFlightPurchaseResponse(response, request);

            string guid = $"{request.Pnr}-{request.DeviceId}";
            string token = await _dPService.GetAnonymousToken(request.Application.Id, request.DeviceId, _configuration).ConfigureAwait(false);
            Dictionary<string, string> captions = await GetSDLCaptions(token);

            bool result = false;
            if (string.Equals("VerifyThisCard", request.VerifyOption, StringComparison.OrdinalIgnoreCase))
            {
                if (state == ContactLessPaymentState.VerifyBookingCard)
                    result = await OptInBookingCardInflightPurchase(request, token, captions);
                else if (state == ContactLessPaymentState.VerifyMPCard)
                    result = await OptInMPCardInflightPurchase(request, token, captions);
                else
                    throw new System.Exception($"Invalid state: {request.State}");
            }
            else if (string.Equals("DonotUseThisCard", request.VerifyOption, StringComparison.OrdinalIgnoreCase)
                || string.Equals("AddDifferentCard", request.VerifyOption, StringComparison.OrdinalIgnoreCase))
            {
                if (state == ContactLessPaymentState.VerifyBookingCard)
                    result = await OptOutBookingCardInflightPurchase(request, token, guid);
                else if (state == ContactLessPaymentState.VerifyMPCard)
                    result = await OptOutMPCardInflightPurchase(request, token);
                else
                    throw new System.Exception($"Invalid state: {request.State}");

                if (result && string.Equals("AddDifferentCard", request.VerifyOption, StringComparison.OrdinalIgnoreCase))
                {
                    response.State = ContactLessPaymentState.AddCard.ToString();
                    UpdateMOBInFlightPurchaseResponse(response, request.FlightSegments, captions);
                    SetAddCardCaptions(response, captions, false);
                    response.PublicDispenserKey = null;
                    if (request.ProductInfo != null && request.ProductInfo.Any())
                    {
                        response.ProductInfo = request.ProductInfo;
                        SetContactLessAODCaptions(response, captions);
                    }
                }
            }
            if (!result)
            {
                throw new MOBUnitedException(_configuration.GetValue<string>("GenericExceptionMessage"));
            }

            return response;
        }

        private async Task<bool> OptOutBookingCardInflightPurchase(MOBSavedCCInflightPurchaseRequest request, string token, string guid)
        {
            string url = "/BackoutFromInFlightUse";
            InflightPurchaseVerifyCardRequest verifyCardRequest = new InflightPurchaseVerifyCardRequest();
            verifyCardRequest.recordLocator = request.Pnr;

            if (_configuration.GetValue<bool>("EnableReplaceVerifyCardObsolete"))
            {
                verifyCardRequest.IsReplacingCard = string.Equals(ContactLessPaymentState.VerifyBookingCard.ToString(), request.State, StringComparison.OrdinalIgnoreCase) && string.Equals("DonotUseThisCard", request.VerifyOption, StringComparison.OrdinalIgnoreCase);
            }

            string jsonRequest = JsonConvert.SerializeObject(verifyCardRequest);
            string cslresponse = await _getTravelersService.OptOutBookingCardInflightPurchase(url, jsonRequest, request.SessionId, token).ConfigureAwait(false);

            if (string.IsNullOrEmpty(cslresponse))
            {
                return false;
            }

            SavePaxWalletResponse response = JsonConvert.DeserializeObject<SavePaxWalletResponse>(cslresponse);

            if (response == null)
            {
                return false;
            }
            if (response.Errors?.Count > 0)
            {
                string errorMsg = String.Join(", ", response.Errors.Select(x => x.MinorDescription));
                _logger.LogWarning("OptOutBookingCardInflightPurchase CSL-CallError ErrorMsg:{errormsg}", errorMsg);

                return false;
            }

            return true;
        }

        private async Task<bool> OptInMPCardInflightPurchase(MOBSavedCCInflightPurchaseRequest request, string token, Dictionary<string, string> captions)
        {
            try
            {
                SavePaxWalletRequest savePaxWalletRequest = await CreateSavePaxWalletRequestToVerifyMPSavedCard(request, false);
                return await SaveCCToPaxWallet(token, savePaxWalletRequest, captions);
            }
            catch (MOBUnitedException uaex)
            {
                //Hack to set MP customer OptInStatus to "OptedOut" when dollar-ding auth failed
                if (uaex.Code == "1000")
                {
                    try { await OptOutMPCardInflightPurchase(request, token); }
                    catch { }
                }
                throw uaex;
            }
            catch (System.Exception ex)
            {
                throw new MOBUnitedException(captions["InflightPurchase_VerifyCard_Error"]);

            }

            return false;
        }

        private async Task<bool> OptOutMPCardInflightPurchase(MOBSavedCCInflightPurchaseRequest request, string token)
        {
            string url = "/OptOutPaxWallet";
            SavePaxWalletRequest savePaxWalletRequest = await CreateSavePaxWalletRequestToVerifyMPSavedCard(request, true);

            string jsonRequest = JsonConvert.SerializeObject(savePaxWalletRequest);
            string cslresponse = await _getTravelersService.OptOutMPCardInflightPurchase(url, jsonRequest, request.SessionId, token).ConfigureAwait(false);

            if (string.IsNullOrEmpty(cslresponse))
            {
                return false;
            }

            SavePaxWalletResponse response = JsonConvert.DeserializeObject<SavePaxWalletResponse>(cslresponse);

            if (response == null)
            {
                return false;
            }

            if (response.Errors?.Count > 0)
            {
                string errorMsg = String.Join(", ", response.Errors.Select(x => x.MinorDescription));
                _logger.LogWarning("OptOutMPCardInflightPurchase CSL-CallError ErrorMsg:{errormsg}", errorMsg);

                return false;
            }

            return true;
        }

        private async Task<SavePaxWalletRequest> CreateSavePaxWalletRequestToVerifyMPSavedCard(MOBSavedCCInflightPurchaseRequest request, bool isOptOutRequested)
        {
            var profileCardResponse = new ProfileCreditCardResponse();
            profileCardResponse = await _sessionHelperService.GetSession<ProfileCreditCardResponse>(request.SessionId, profileCardResponse.ObjectName, new List<string> { request.SessionId, profileCardResponse.ObjectName }).ConfigureAwait(false);

            if (profileCardResponse == null)
            {
                throw new MOBUnitedException(_configuration.GetValue<string>("ViewResSessionExpiredMessage"));
            }

            OptInType optInType = IsHomeScreenFlow(request.Flow) ? OptInType.PostCheckin : OptInType.Checkin;
            string optOutPaxMPNumber = isOptOutRequested ? profileCardResponse.MileagePlusNumber : string.Empty;
            SavePaxWalletRequest savePaxWalletRequest = CreateSavePaxWalletRequest(request, optInType, optOutPaxMPNumber);
            savePaxWalletRequest.CreditCard = profileCardResponse.PrimaryCreditCard;
            savePaxWalletRequest.PointOfSaleCountryCode = "US";
            if (isOptOutRequested)
            {
                savePaxWalletRequest.PaxToPay.FirstOrDefault().CreditCard = profileCardResponse.PrimaryCreditCard;
            }

            return savePaxWalletRequest;
        }

        private async Task<bool> OptInBookingCardInflightPurchase(MOBSavedCCInflightPurchaseRequest request, string token, Dictionary<string, string> captions)
        {
            if (((request.FlightSegments?.Count() ?? 0) == 0)
                        || (request.FlightSegments.Any(seg => (seg.TravelersDetails?.Count() ?? 0) == 0)))
            {
                throw new System.Exception("Invalid request: Missing either FlightSegments or TravelersDetails data");
            }

            SavePaxWalletResponse response = null;
            try
            {
                string url = "/ConfirmForInFlightUse";
                InflightPurchaseVerifyCardRequest verifyCardRequest = new InflightPurchaseVerifyCardRequest();
                verifyCardRequest.recordLocator = request.Pnr;
                verifyCardRequest.PaxNames = request.FlightSegments.First().TravelersDetails
                                                    .Select(trvl => new PaxName() { FirstName = trvl.FirstName, LastName = trvl.LastName }).ToList();
                string departureDate(string date)
                {
                    DateTime dt;
                    DateTime.TryParse(date, out dt);
                    return string.Format("{0:MM/dd/yyyy}", dt);
                }
                verifyCardRequest.Segments = request.FlightSegments.Where(seg => seg.IsEligible).Select(seg => new Segments
                {
                    FlightNumber = $"{seg.CarrierCode}{seg.FlightNumber}",
                    DepartureDate = departureDate(seg.DepartureDate),
                    Origin = seg.DepartureAirport
                }).ToList();

                string jsonRequest = JsonConvert.SerializeObject(verifyCardRequest);
                string jsonResponse = await GetCslApiResponse(url, token, jsonRequest);
                if (string.IsNullOrEmpty(jsonResponse))
                {
                    return false;
                }

                response = JsonConvert.DeserializeObject<SavePaxWalletResponse>(jsonResponse);

                if (response == null)
                {
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                throw new MOBUnitedException(captions["InflightPurchase_VerifyCard_Error"]);
            }

            return CheckAuthError(response, captions);
        }

        private async Task<MOBInFlightPurchaseResponse> AddOrReplaceCCForInflightPurchase(MOBSavedCCInflightPurchaseRequest request)
        {
            MOBInFlightPurchaseResponse response = new MOBInFlightPurchaseResponse();
            PopulateMOBInFlightPurchaseResponse(response, request);

            string guid = $"{request.Pnr}-{request.DeviceId}";
            string token = await _dPService.GetAnonymousToken(request.Application.Id, request.DeviceId, _configuration).ConfigureAwait(false);
            Dictionary<string, string> captions = await GetSDLCaptions(token);

            if (await GenerateCCTokenWithDataVault(request.FormofPaymentDetails.CreditCard, guid, token, request.Application, request.DeviceId))
            {
                if (string.Equals
                    (request.FormofPaymentDetails.BillingAddress.Country.Code, "US", StringComparison.OrdinalIgnoreCase))
                {
                    string stateCode = string.Empty;
                    bool isValidStateCode = false;
                    (isValidStateCode, stateCode) = await GetAndValidateStateCode_CFOP(request, token, guid, request.FormofPaymentDetails.BillingAddress.State.Code, request.FormofPaymentDetails.BillingAddress.Country.Code);
                    if (isValidStateCode)
                    {
                        request.FormofPaymentDetails.BillingAddress.State.Code = stateCode;
                    }
                }

                OptInType optInType = IsHomeScreenFlow(request.Flow) ? OptInType.PostCheckin : OptInType.Checkin;
                SavePaxWalletRequest savePaxWalletRequest = CreateSavePaxWalletRequest(request, optInType);
                savePaxWalletRequest.PointOfSaleCountryCode = "US";
                savePaxWalletRequest.CreditCard = new CreditCard
                {
                    AccountNumberLastFourDigits = request.FormofPaymentDetails.CreditCard.DisplayCardNumber.Substring(request.FormofPaymentDetails.CreditCard.DisplayCardNumber.Length - 4),
                    Address = new Definition.TouchlessPayments.Address()
                    {
                        city = request.FormofPaymentDetails.BillingAddress.City,
                        country = request.FormofPaymentDetails.BillingAddress.Country.Code,
                        postcodeZip = request.FormofPaymentDetails.BillingAddress.PostalCode,
                        stateProvince = request.FormofPaymentDetails.BillingAddress.State.Code,
                        street = request.FormofPaymentDetails.BillingAddress.Line1,
                        street2 = request.FormofPaymentDetails.BillingAddress.Line2,
                        street3 = request.FormofPaymentDetails.BillingAddress.Line3
                    },
                    CreditCardCode = request.FormofPaymentDetails.CreditCard.CardType,
                    CurrencyCode = "USD",
                    ExpiryMonthYear = request.FormofPaymentDetails.CreditCard.ExpireMonth + request.FormofPaymentDetails.CreditCard.ExpireYear,
                    PersistentToken = request.FormofPaymentDetails.CreditCard.PersistentToken,
                    SecureCodeToken = request.FormofPaymentDetails.CreditCard.SecurityCodeToken,
                    NameOnCard = request.FormofPaymentDetails.CreditCard.CCName
                };

                //EnableContactLessSaveToProfileForPayment
                var expireYear = request.FormofPaymentDetails.CreditCard.ExpireYear.Count() > 3 ? request.FormofPaymentDetails?.CreditCard?.ExpireYear.Substring(2, 2) : request.FormofPaymentDetails.CreditCard.ExpireYear;
                savePaxWalletRequest.CreditCard.ExpiryMonthYear = $"{int.Parse(request.FormofPaymentDetails?.CreditCard?.ExpireMonth):D2}{expireYear}";

                bool result = await SaveCCToPaxWallet(token, savePaxWalletRequest, captions);
                if (!result)
                {
                    throw new MOBUnitedException(_configuration.GetValue<string>("GenericExceptionMessage"));
                }

                if (IsSaveCCToProfile_ContactlessEnabled(request.Application) && request.IsSavedToProfile)
                {
                    List<MOBItem> insertUpdateItemKeys = new List<MOBItem>();
                    var res = await SaveCCToProfile(request, token, captions);
                }
            }

            _logger.LogInformation("AddOrReplaceCCForInflightPurchase Response:{response}", JsonConvert.SerializeObject(response));

            return response;
        }

        public T ObjectToObjectCasting<T, R>(R request)
        {
            var typeInstance = Activator.CreateInstance(typeof(T));

            foreach (var propReq in request.GetType().GetProperties())
            {
                var propRes = typeInstance.GetType().GetProperty(propReq.Name);
                if (propRes != null)
                {
                    propRes.SetValue(typeInstance, propReq.GetValue(request));
                }
            }

            return (T)typeInstance;
        }

        private async Task<bool> SaveCCToProfile(MOBSavedCCInflightPurchaseRequest request, string token, Dictionary<string, string> captions)
        {
            string url = "/CustomerProfile/SaveCCToProfile";
            MOBUpdateProfileOwnerFOPRequest updateProfileOwnerRequest = ObjectToObjectCasting<MOBUpdateProfileOwnerFOPRequest, MOBSavedCCInflightPurchaseRequest>(request);
            updateProfileOwnerRequest.Token = token;
            string jsonRequest = JsonConvert.SerializeObject(updateProfileOwnerRequest);

            string jsonResponse = await GetCslApiResponse(url, token, jsonRequest);

            if (string.IsNullOrEmpty(jsonResponse))
            {
                return false;
            }

            SavePaxWalletResponse response = JsonConvert.DeserializeObject<SavePaxWalletResponse>(jsonResponse);
            if (response == null)
            {
                return false;
            }
            return CheckAuthError(response, captions);
        }

        private bool IsSaveCCToProfile_ContactlessEnabled(MOBApplication application)
        {
            if (_configuration.GetValue<bool>("EnableSaveCCToProfile_Contactless"))
            {
                return true;
            }
            return false;
        }

        private SavePaxWalletRequest CreateSavePaxWalletRequest(MOBSavedCCInflightPurchaseRequest request, OptInType optInType, string optOutPaxMPNumber = null)
        {
            if (((request.FlightSegments?.Count() ?? 0) == 0)
                        || (request.FlightSegments.Any(seg => (seg.TravelersDetails?.Count() ?? 0) == 0)))
            {
                throw new System.Exception("Invalid request: Missing either FlightSegments or TravelersDetails data");
            }

            SavePaxWalletRequest savePaxWalletRequest = new SavePaxWalletRequest
            {
                OptInType = optInType,
                ChannelName = request.Application.Name,
                RecordLocator = request.Pnr,
                PaymentOrigin = optInType.ToString(),
                IPAddress = _configuration.GetValue<string>("SaveWalletStaticIpAddress")
            };
            string departureDate(string date)
            {
                DateTime dt;
                DateTime.TryParse(date, out dt);
                return string.Format("{0:MM/dd/yyyy}", dt);
            }
            savePaxWalletRequest.Segments = request.FlightSegments.Where(seg => seg.IsEligible).Select(seg => new Segments
            {
                FlightNumber = $"{seg.CarrierCode}{seg.FlightNumber}",
                DepartureDate = departureDate(seg.DepartureDate),
                Origin = seg.DepartureAirport
            }).ToList();

            if (!string.IsNullOrEmpty(optOutPaxMPNumber))
            {
                MOBInflightPurchaseTravelerInfo optOutTravelerInfo = request.FlightSegments.FirstOrDefault().TravelersDetails
                                                                            .FirstOrDefault(trvl => string.Equals(optOutPaxMPNumber, trvl.FqtvNumber, StringComparison.OrdinalIgnoreCase));
                savePaxWalletRequest.PaxToPay = new List<PaxToPay>() { new PaxToPay() { FirstName = optOutTravelerInfo.FirstName, LastName = optOutTravelerInfo.LastName } };
            }
            else
            {
                savePaxWalletRequest.PaxToPay = request.FlightSegments.First().TravelersDetails.Select(trvl => new PaxToPay
                {
                    FirstName = trvl.FirstName,
                    LastName = trvl.LastName,
                    OptInType = optInType
                }).ToList();
            }

            return savePaxWalletRequest;
        }

        private async Task<bool> SaveCCToPaxWallet(string token, SavePaxWalletRequest savePaxWalletRequest, Dictionary<string, string> captions)
        {
            string url = "/SavePaxWallet";
            string jsonRequest = JsonConvert.SerializeObject(savePaxWalletRequest);

            string jsonResponse = await GetCslApiResponse(url, token, jsonRequest);

            if (string.IsNullOrEmpty(jsonResponse))
            {
                return false;
            }

            SavePaxWalletResponse response = JsonConvert.DeserializeObject<SavePaxWalletResponse>(jsonResponse);

            if (response == null)
            {
                return false;
            }

            return CheckAuthError(response, captions);
        }

        private async Task<string> GetCslApiResponse(string url, string token, string jsonRequest)
        {
            string jsonResponse = await _getTravelersService.GetCslApiResponse(url, jsonRequest, _headers.ContextValues.SessionId, token).ConfigureAwait(false);
            return jsonResponse;
        }

        private bool CheckAuthError(SavePaxWalletResponse response, Dictionary<string, string> captions)
        {
            if (response.Errors?.Count > 0)
            {
                string errorMsg = String.Join(", ", response.Errors.Select(x => x.MinorDescription));
                _logger.LogWarning("CSL-CallError ErrorMsg:{errormsg}", errorMsg);

                if (response.Errors.Any(e => "109" == e.MinorCode))
                {
                    MOBUnitedException exception = new MOBUnitedException(captions["InflightPurchase_Card_Auth_Error"]);
                    exception.Code = "1000";
                    throw exception;
                }

                return false;
            }

            return true;
        }

        private bool IsHomeScreenFlow(string flowName)
        {
            FlowType flowType;
            if (!Enum.TryParse(flowName, out flowType))
                return false;

            return flowType == FlowType.HOMESCREEN;
        }

        private async Task<bool> GenerateCCTokenWithDataVault(MOBCreditCard creditCardDetails, string sessionID, string token, MOBApplication applicationDetails, string deviceID)
        {
            bool generatedCCTokenWithDataVault = false;
            if (!string.IsNullOrEmpty(creditCardDetails.EncryptedCardNumber)) // expecting Client will send only Encrypted Card Number only if the user input is a clear text CC number either for insert CC or update CC not the CC details like CVV number update or expiration date upate no need to call data vault for this type of updates only data vault will be called for CC number update to get the CC token back
            {
                #region
                CslDataVaultRequest dataVaultRequest = GetDataValutRequest(creditCardDetails, sessionID, applicationDetails.Id);
                string jsonRequest = DataContextJsonSerializer.Serialize<CslDataVaultRequest>(dataVaultRequest);
                string jsonResponse = await _dataVaultService.GetCCTokenWithDataVault(token, jsonRequest, sessionID).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(jsonResponse))
                {
                    CslDataVaultResponse response = DataContextJsonSerializer.DeserializeJsonDataContract<CslDataVaultResponse>(jsonResponse);
                    if (response != null && response.Responses != null && response.Responses[0].Error == null && response.Responses[0].Message != null && response.Responses[0].Message.Count > 0 && response.Responses[0].Message[0].Code.Trim() == "0")
                    {
                        generatedCCTokenWithDataVault = true;
                        var creditCard = response.Items[0] as Service.Presentation.PaymentModel.CreditCard;
                        creditCardDetails.AccountNumberToken = creditCard.AccountNumberToken;
                        creditCardDetails.PersistentToken = creditCard.PersistentToken;
                        creditCardDetails.SecurityCodeToken = creditCard.SecurityCodeToken;
                        if (String.IsNullOrEmpty(creditCardDetails.CardType))
                        {
                            creditCardDetails.CardType = creditCard.Code;
                        }
                        else if (!_configuration.GetValue<bool>("DisableCheckForUnionPayFOP_MOBILE13762"))
                        {
                            string[] checkForUnionPayFOP = _configuration.GetValue<string>("CheckForUnionPayFOP")?.Split('|');
                            if (creditCard?.Code == checkForUnionPayFOP?[0])
                            {
                                creditCardDetails.CardType = creditCard.Code;
                                creditCardDetails.CardTypeDescription = checkForUnionPayFOP?[1];
                            }
                        }
                    }
                    else
                    {
                        if (response.Responses[0].Error != null && response.Responses[0].Error.Count > 0)
                        {
                            string errorMessage = string.Empty;
                            string errorCode = string.Empty;
                            foreach (var error in response.Responses[0].Error)
                            {
                                errorMessage = errorMessage + " " + error.Text;
                                errorCode = error.Code;
                            }
                            throw new MOBUnitedException(errorCode, errorMessage);
                        }
                        else
                        {
                            string exceptionMessage = _configuration.GetValue<string>("UnableToInsertCreditCardToProfileErrorMessage");
                            if (_configuration.GetValue<string>("ReturnActualExceptionMessageBackForTesting") != null && Convert.ToBoolean(_configuration.GetValue<string>("ReturnActualExceptionMessageBackForTesting")))
                            {
                                exceptionMessage = exceptionMessage + " response.Status not success and response.Errors.Count == 0 - at DAL InsertTravelerCreditCard(MOBUpdateTravelerRequest request)";
                            }
                            throw new MOBUnitedException(exceptionMessage);
                        }
                    }
                }
                else
                {
                    string exceptionMessage = _configuration.GetValue<string>("UnableToInsertCreditCardToProfileErrorMessage");
                    if (_configuration.GetValue<string>("ReturnActualExceptionMessageBackForTesting") != null && _configuration.GetValue<bool>("ReturnActualExceptionMessageBackForTesting"))
                    {
                        exceptionMessage = exceptionMessage + " - due to jsonResponse is empty at DAL  GenerateCCTokenWithDataVault(MOBUpdateTravelerRequest request, ref string ccDataVaultToken)";
                    }
                    throw new MOBUnitedException(exceptionMessage);
                }
                #endregion
            }
            else if (!string.IsNullOrEmpty(creditCardDetails.AccountNumberToken.Trim()))
            {
                generatedCCTokenWithDataVault = true;
            }

            return generatedCCTokenWithDataVault;
        }

        private CslDataVaultRequest GetDataValutRequest(MOBCreditCard creditCardDetails, string sessionID, int appId)
        {
            #region
            var dataVaultRequest = new CslDataVaultRequest
            {
                Items = new Collection<United.Service.Presentation.PaymentModel.Payment>(),
                Types = new Collection<Characteristic>(),
                CallingService = new ServiceClient { Requestor = new Requestor { AgentAAA = "WEB", ApplicationSource = "mobile services" } }
            };
            InsertCreditCardRequest creditCardInsertRequest = new InsertCreditCardRequest();
            if (creditCardDetails != null)
            {
                var cc = new United.Service.Presentation.PaymentModel.CreditCard();
                cc.ExpirationDate = creditCardDetails.ExpireMonth + "/" + (creditCardDetails.ExpireYear.Trim().Length == 2 ? creditCardDetails.ExpireYear.Trim() : creditCardDetails.ExpireYear.Trim().Substring(2, 2).ToString()); //"05/17";
                if (!_configuration.GetValue<bool>("DisableUATBCvvCodeNullCheckAndAssignEmptyString") &&
                  string.IsNullOrEmpty(creditCardDetails.CIDCVV2))
                {
                    cc.SecurityCode = "";
                }
                else
                {
                    cc.SecurityCode = creditCardDetails.CIDCVV2.Trim(); //"1234";
                }

                cc.Code = creditCardDetails.CardType;  //"VI";
                cc.Name = creditCardDetails.CCName; //"Test Testing";
                if (!string.IsNullOrEmpty(creditCardDetails.EncryptedCardNumber))
                {
                    dataVaultRequest.Types = new Collection<Characteristic>();
                    dataVaultRequest.Types.Add(new Characteristic { Code = "ENCRYPTION", Value = "PKI" });
                    cc.AccountNumberEncrypted = creditCardDetails.EncryptedCardNumber;
                    if (_configuration.GetValue<bool>("EnablePKDispenserKeyRotationAndOAEPPadding") && creditCardDetails.IsOAEPPaddingCatalogEnabled)
                    {
                        if (string.IsNullOrEmpty(creditCardDetails?.Kid))
                        {
                            //    string transId = string.IsNullOrEmpty(_headers.ContextValues.TransactionId) ? "trans0" : _headers.ContextValues.TransactionId;
                            //    string key = string.Format(_configuration.GetValue<string>("PKDispenserKeyTokenKeyFormat"),appId);
                            //    var cacheResponse = await _cachingService.GetCache<United.Service.Presentation.SecurityResponseModel.PKDispenserKey>(key, transId).ConfigureAwait(false);
                            //    var obj = JsonConvert.DeserializeObject<United.Service.Presentation.SecurityResponseModel.PKDispenserKey>(cacheResponse);
                            //    dataVaultRequest.Types.Add(new Characteristic { Code = "KID", Value = obj.Kid });

                            _logger.LogError("GetCreditCardToken Exception-kid value in request is empty with {sessionId}", sessionID);
                            throw new Exception(_configuration.GetValue<string>("GenericExceptionMessage"));
                        }
                        else
                        {
                            dataVaultRequest.Types.Add(new Characteristic { Code = "KID", Value = creditCardDetails.Kid });
                        }
                        dataVaultRequest.Types.Add(new United.Service.Presentation.CommonModel.Characteristic { Code = "OAEP", Value = "TRUE" });

                    }
                }
                else
                {
                    cc.AccountNumber = creditCardDetails.UnencryptedCardNumber; //"4000000000000002";
                }

                if (_configuration.GetValue<bool>("PassMobileSessionIDInsteadOfDifferntGuidEveryTime"))
                {
                    cc.OperationID = sessionID.ToString(); // This one we can pass the session id which we using in bookign path.
                }
                else if (_configuration.GetValue<bool>("EDDtoEMDToggle"))
                {
                    cc.OperationID = Guid.NewGuid().ToString(); // This one we can pass the session id which we using in bookign path.
                }
                else
                {
                    cc.OperationID = Guid.NewGuid().ToString().ToUpper().Replace("-", "");
                }
                ///93875 - iOS Android UnionPay bookings are not throwing error for bin ranges that are not 16 digit credit cards
                ///Srini - 02/13/2018
                if (_configuration.GetValue<bool>("DataVaultRequestAddDollarDingToggle"))
                {
                    cc.PinEntryCapability = Service.Presentation.CommonEnumModel.PosTerminalPinEntryCapability.PinNotSupported;
                    cc.TerminalAttended = Service.Presentation.CommonEnumModel.PosTerminalAttended.Unattended;
                    dataVaultRequest.PointOfSaleCountryCode = "US";
                    dataVaultRequest.Types.Add(new Characteristic { Code = "DOLLAR_DING", Value = "TRUE" });
                }
                dataVaultRequest.Items.Add(cc);
            }

            return dataVaultRequest;
            #endregion
        }

        private async Task<(bool, string)> GetAndValidateStateCode_CFOP(MOBSavedCCInflightPurchaseRequest request, string token, string guid, string stCode, string countryCode)
        {
            bool validStateCode = false;
            string stateCode = string.Empty;
            #region
            //http://unitedservicesstage.ual.com/5.0/referencedata/StatesFilter?State=tex&CountryCode=US&Language=en-US
            string path = string.Format("/StatesFilter?State={0}&CountryCode={1}&Language={2}", request.FormofPaymentDetails.BillingAddress.State.Code, request.FormofPaymentDetails.BillingAddress.Country.Code, request.LanguageCode);

            var response = await _referencedataService.GetDataGetAsync<List<StateProvince>>(path, token, string.Empty).ConfigureAwait(false);
            if (response != null)
            {
                if (response != null && response.Count == 1 && !string.IsNullOrEmpty(response[0].StateProvinceCode))
                {
                    stateCode = response[0].StateProvinceCode;
                    validStateCode = true;
                }
                else
                {
                    string exceptionMessage = _configuration.GetValue<string>("UnableToGetAndValidateStateCode").ToString();
                    throw new MOBUnitedException(exceptionMessage);
                }
            }
            else
            {
                string exceptionMessage = _configuration.GetValue<string>("UnableToGetAndValidateStateCode");
                if (_configuration.GetValue<string>("ReturnActualExceptionMessageBackForTesting") != null && Convert.ToBoolean(_configuration.GetValue<string>("ReturnActualExceptionMessageBackForTesting").ToString()))
                {
                    exceptionMessage = exceptionMessage + " - due to jsonResponse is empty at DAL  GetCommonUsedDataList()";
                }
                throw new MOBUnitedException(exceptionMessage);
            }
            #endregion

            return (validStateCode, stateCode);
        }

        private async Task GetCLPBillingAddressCountries(MOBInFlightPurchaseResponse response)
        {
            var countriesList = new List<MOBItem>();

            var billingCountries = await GetCachedBillingAddressCountries();

            if (billingCountries == null || !billingCountries.Any())
            {
                billingCountries = new List<MOBCPBillingCountry>();

                billingCountries.Add(new MOBCPBillingCountry
                {
                    CountryCode = "US",
                    CountryName = "United States",
                    Id = "1",
                    IsStateRequired = true,
                    IsZipRequired = true
                });
            }

            billingCountries?.ForEach(ctry =>
            {
                if (ctry != null)
                {
                    countriesList.Add(new MOBItem
                    {
                        Id = ctry.CountryCode,
                        CurrentValue = ctry.CountryName
                    });
                }
            });

            response.BillingAddressCountries
                = (countriesList == null || !countriesList.Any()) ? null : countriesList;

            response.BillingAddressProperties
                = (billingCountries == null || !billingCountries.Any()) ? null : billingCountries;
        }

        private async Task<List<MOBCPBillingCountry>> GetCachedBillingAddressCountries()
        {
            try
            {
               return await _inflightDynamoDB.GetInflightContent<List<MOBCPBillingCountry>>("inflight-billingcountries");
            }
            catch (MOBUnitedException uex)
            {
                _logger.LogError("Unable to retrieve data for billing countries from auroradb");
                throw uex;
            }

            return null;
        }

        private async Task<MOBInFlightPurchaseResponse> EligibilityCheckForContactlessPaymentInFlightPurchase(MOBInFlightPurchaseRequest request)
        {
            MOBInFlightPurchaseResponse response = new MOBInFlightPurchaseResponse();

            PopulateMOBInFlightPurchaseResponse(response, request);

            if (((request.FlightSegments?.Count() ?? 0) == 0)
                    || (request.FlightSegments.Any(seg => (seg.TravelersDetails?.Count() ?? 0) == 0)))
            {
                throw new System.Exception("Invalid request: Missing either FlightSegments or TravelersDetails data");
            }

            if ((request.FlightSegments?.FirstOrDefault()?.TravelersDetails?.Count() ?? 0) > 9) { return response; }

            string guid = $"{request.Pnr}-{request.DeviceId}";
            string token = await _dPService.GetAnonymousToken(request.Application.Id, request.DeviceId, _configuration).ConfigureAwait(false);
            Dictionary<string, string> captions = await GetSDLCaptions(token);

            Collection<ProductSegment> productSegments = new Collection<ProductSegment>();

            //1. Check if trip eligible for Inflight purchases
            string flightContactlessProgram = _configuration.GetValue<string>("FlightContactlessProgram");
            productSegments = await GetInflightPurchaseEligibility(request, token, guid, flightContactlessProgram);

            response.ProductInfo = GetFlightProductInfo(productSegments, flightContactlessProgram, captions["InflightPurchaseProductSection_OnboardLabel"], captions["InflightPurchaseProductSection_AODLabel"]);
            if (!response.ProductInfo.Any())
            {
                return response;
            }

            SetContactLessAODCaptions(response, captions);

            SetContactLesStatusCaptions(response, captions, productSegments, flightContactlessProgram);

            bool doesMatch(MOBInflightPurchaseFlightSegments fs, ProductSegment ps) => fs.DepartureAirport == ps.DepartureAirport.IATACode
                && fs.ArrivalAirport == ps.ArrivalAirport.IATACode;
            productSegments.ForEach(ps => { request.FlightSegments.FirstOrDefault(fs => doesMatch(fs, ps)).IsEligible = ps.IsRulesEligible.ToBoolean(); });
            UpdateMOBInFlightPurchaseResponse(response, request.FlightSegments, captions);

            //2. Get saved cards from Touchless Payment Wallet lookup service
            PaxWalletLookupResponse walletLookupResponse = await GetTravelersSavedCCDetails(request, token, request.Pnr);

            #region Verify card saved during booking
            bool isBookingCardExpired = false;
            List<SegmentsToPay> segmentsToPayList = GetSegmentsToPayMatchingWithRequestSegments(request.FlightSegments, walletLookupResponse?.SegmentsToPay);
            List<PaxToPay> paxToPayListAll = segmentsToPayList?.SelectMany(seg => seg.PaxToPay).ToList();
            if (paxToPayListAll?.All(ptp => DoesTravelerHasBookingSavedCard(ptp)) == true)
            {
                List<PaxToPay> paxToPayList = new List<PaxToPay>();
                foreach (PaxToPay paxToPay in paxToPayListAll)
                {
                    if (DoesTravelerHasBookingSavedCard(paxToPay))
                    {
                        if (!IsCardValid(paxToPay.CreditCard))
                        {
                            isBookingCardExpired = true;
                            paxToPayList.Clear();
                            break;
                        }
                        paxToPayList.Add(paxToPay);
                    }
                }

                if (paxToPayList?.Any() == true)
                {
                    PaxToPay paxToPay = GetTravelerWithBookingSavedCard(request, response, paxToPayList);
                    if (paxToPay != null)
                    {
                        response.State = ContactLessPaymentState.VerifyBookingCard.ToString();
                        response.SavedCreditcardsInfo = new List<MOBSavedCCInflightPurchase>()
                        {
                            new MOBSavedCCInflightPurchase()
                            {
                                CardInformation = $"{GetCardDescription(paxToPay.CreditCard.CreditCardCode)} **{paxToPay.CreditCard.AccountNumberLastFourDigits}",
                                Travelers = paxToPay.CreditCard.NameOnCard,
                                Flights = response.EligibleSegments,
                                PersistentToken = paxToPay.CreditCard.PersistentToken
                            }
                        };
                        response.AllowPaxSelection = false;
                        SetVerifyCardCaptions(response, captions);
                        SetRTDDisplayPopupCaptions(request, response, captions);

                        return response;
                    }
                }
            }
            #endregion Verify card saved during booking

            #region Verify card saved in MP profile
            if (IsMpLoggedIn(request))
            {
                //3. Get MP# for Employee id from Loyalty service
                List<string> employeeIds = GetEmployeeIds(request);
                if ((employeeIds?.Count ?? 0) > 0)
                {
                    foreach (var empId in employeeIds)
                    {
                        string employeeMpNumber = await GetMPNumberByEmployeeId(request, empId, token, guid);
                        if (string.Equals(request.MileagePlusNumber, employeeMpNumber, StringComparison.OrdinalIgnoreCase))
                        {
                            UpdateEmployeeMpNumber(request, empId, employeeMpNumber);
                            break;
                        }
                    }
                }

                //4. Get CC for logged in MP from Profile service
                MOBInflightPurchaseTravelerInfo mpTravelerInfo = request.FlightSegments.SelectMany(seg => seg.TravelersDetails)
                                                                                       .FirstOrDefault(trvl => string.Equals(request.MileagePlusNumber, trvl.FqtvNumber, StringComparison.OrdinalIgnoreCase));

                if (mpTravelerInfo != null)
                {
                    PaxToPay mpPaxToPay = segmentsToPayList?.SelectMany(seg => seg.PaxToPay)?
                                                            .FirstOrDefault(pax => string.Equals(mpTravelerInfo.LastName, pax.LastName, StringComparison.OrdinalIgnoreCase)
                                                                               && string.Equals(mpTravelerInfo.FirstName, pax.FirstName, StringComparison.OrdinalIgnoreCase));
                    if (mpPaxToPay == null || (!string.Equals("Confirmed", mpPaxToPay?.OptInStatus, StringComparison.OrdinalIgnoreCase)
                                                && !string.Equals("OptedOut", mpPaxToPay?.OptInStatus, StringComparison.OrdinalIgnoreCase)
                                                && !string.Equals("Pending", mpPaxToPay?.OptInStatus, StringComparison.OrdinalIgnoreCase)))
                    {
                        Tuple<ProfileCreditCard, ProfileCreditCardResponse> profileCardDetaiils = await GetLoggedInMPSavedCard(request, token, guid);
                        if (profileCardDetaiils != null)
                        {
                            //Do not present MP saved card for verification
                            if (_configuration.GetValue<bool>("EnableLiftMPSavedCardVerification"))
                            {
                                walletLookupResponse = await LiftMPSavedCardVerification(request, guid, token, profileCardDetaiils.Item2);
                            }
                            else
                            {
                                ProfileCreditCard profileCard = profileCardDetaiils.Item1;
                                response.State = ContactLessPaymentState.VerifyMPCard.ToString();
                                response.SavedCreditcardsInfo = new List<MOBSavedCCInflightPurchase>()
                                    {
                                        new MOBSavedCCInflightPurchase()
                                        {
                                            CardInformation = GetCardInfo(profileCard.CCTypeDescription, profileCard.AccountNumberMasked),
                                            Travelers = $"{mpTravelerInfo.FirstName} {mpTravelerInfo.LastName}",
                                            Flights = response.EligibleSegments,
                                            PersistentToken = mpPaxToPay.CreditCard.PersistentToken
                                        }
                                    };
                                SetVerifyCardCaptions(response, captions);
                                SetRTDDisplayPopupCaptions(request, response, captions);

                                return response;
                            }
                        }
                    }
                }
            }
            #endregion Verify card saved in MP profile

            bool isOtherTravelerHaveSavedCC = false;
            response.SavedCreditcardsInfo = GetTravelersSavedCreditCardDetails(walletLookupResponse, response.FlightSegments, ref isOtherTravelerHaveSavedCC);

            if (response.SavedCreditcardsInfo.Count > 0)
            {
                response.State = ContactLessPaymentState.ReviewCard.ToString();
                SetReviewCardCaptions(response, captions);
            }
            else
            {
                response.State = ContactLessPaymentState.AddCard.ToString();
                SetAddCardCaptions(response, captions, isBookingCardExpired);
                SetRTDDisplayPopupCaptions(request, response, captions);
            }
            PopulateAttentionMessages(response, !Convert.ToBoolean(request.AllPaxInTrans), isOtherTravelerHaveSavedCC, isBookingCardExpired, captions);

            response.PublicDispenserKey = null;

            return response;
        }


        private void PopulateAttentionMessages(MOBInFlightPurchaseResponse response, bool isPartialCheckin, bool isOtherTravelersHaveSavedCC, bool isBookingCardExpired, Dictionary<string, string> captions)
        {
            List<string> messages = new List<string>();

            if (isBookingCardExpired)
            {
                messages.Add(GetBookingCardExpiredCaption(captions));
            }

            if (!isOtherTravelersHaveSavedCC && isPartialCheckin)
            {
                messages.Add(captions["InflightPurchase_AttentionMessage-PartialCheckin"]);
            }

            if (isOtherTravelersHaveSavedCC)
            {
                messages.Add(captions["InflightPurchase_AttentionMessage-PartialCheckin-AddedCCInSeparateTransaction"]);
            }

            if (response.SavedCreditcardsInfo != null && response.SavedCreditcardsInfo.Count > 0 && response.FlightSegments.Count > 1)
            {
                List<string> originsDestionations = new List<string>();
                response.FlightSegments.Where(seg => seg.IsEligible).ForEach(fs =>
                {
                    if (!fs.TravelersDetails.Exists(t => t.HasSavedCreditCard))
                    {
                        originsDestionations.Add($"{fs.DepartureAirport}-{fs.ArrivalAirport}");
                    }
                });
                if (originsDestionations.Count > 0)
                {
                    messages.Add(string.Format(captions["InflightPurchase_AttentionMessage-AddCCOtherSegments"], string.Join(",", originsDestionations)));
                }
            }

            response.AttentionMessages.InsertRange(0, messages);
        }

        private void SetAddCardCaptions(MOBInFlightPurchaseResponse response, Dictionary<string, string> captions, bool isBookingCardExpired)
        {
            SetAddCardCommonCaptions(response, captions);
            response.Captions.Add(new MOBItem() { Id = "InflightPurchaseSection_HeaderLabel", CurrentValue = captions["InflightPurchaseSection_HeaderLabel"] });
            response.Captions.Add(new MOBItem() { Id = "InflightPurchaseCCDetails_HeaderTitle", CurrentValue = captions["InflightPurchaseCCDetails_HeaderTitle"] });
            response.Captions.Add(new MOBItem() { Id = "InflightPurchasePickPaxSection_HeaderLabbel", CurrentValue = captions["InflightPurchasePickPaxSection_HeaderLabbel"] });
            response.Captions.Add(new MOBItem() { Id = "InflightPurchasePickPaxButton_Continue", CurrentValue = captions["InflightPurchasePickPaxButton_Continue"] });
            response.Captions.Add(new MOBItem() { Id = "InflightPurchasePickPaxButton_Cancel", CurrentValue = captions["InflightPurchasePickPaxButton_Cancel"] });

            if (isBookingCardExpired)
            {
                response.Captions.Add(new MOBItem() { Id = "InflightPurchasePickPaxSection_InfoAlert", CurrentValue = GetBookingCardExpiredCaption(captions) });
                response.Captions.Add(new MOBItem() { Id = "InflightPurchasePickPaxSection_AlertIconName", CurrentValue = "warning" });
            }
            else
            {
                response.Captions.Add(new MOBItem() { Id = "InflightPurchasePickPaxSection_InfoAlert", CurrentValue = captions["InflightPurchasePickPaxSection_InfoAlert"] });
            }
            response.Captions.Add(new MOBItem() { Id = "InflightPurchasePickPax_Error", CurrentValue = captions["InflightPurchasePickPax_Error"] });
        }

        private string GetBookingCardExpiredCaption(Dictionary<string, string> captions)
        {
            return (captions.ContainsKey("InflightPurchaseBookingExpiredCard_WarningAlert") ? captions["InflightPurchaseBookingExpiredCard_WarningAlert"] : "The card you've saved for contactless payment has or is about to expire. Please continue to add a new card.");
        }

        private void SetReviewCardCaptions(MOBInFlightPurchaseResponse response, Dictionary<string, string> captions)
        {
            SetAddCardCommonCaptions(response, captions);
            SetTripTipCaptions(response, captions);
            response.Captions.Add(new MOBItem() { Id = "InflightPurchaseSavedCCSection_FlightsLabel", CurrentValue = captions["InflightPurchaseSavedCCSection_FlightsLabel"] });
            response.Captions.Add(new MOBItem() { Id = "InflightPurchasePickPaxSection_HeaderLabbel", CurrentValue = captions["InflightPurchase_ReviewCard_PickPax_HeaderLabel"] });
            if (response.AllowPaxSelection)
                response.Captions.Add(new MOBItem() { Id = "InflightPurchasePickPaxSection_InfoAlert", CurrentValue = captions["InflightPurchase_ReviewCard_PickPax_InfoAlert"] });

            if (response.FlightSegments?.FirstOrDefault()?.TravelersDetails?.All(trvl => trvl.HasSavedCreditCard) ?? false)
                response.Captions.Add(new MOBItem() { Id = "InflightPurchasePickPaxButton_Continue", CurrentValue = captions.ContainsKey("InflightPurchasePickPaxButton_EditCard") ? captions["InflightPurchasePickPaxButton_EditCard"] : "Edit card" });
            else
                response.Captions.Add(new MOBItem() { Id = "InflightPurchasePickPaxButton_Continue", CurrentValue = captions.ContainsKey("InflightPurchasePickPaxButton_AddOrEditCard") ? captions["InflightPurchasePickPaxButton_AddOrEditCard"] : "Add or edit card" });

            response.Captions.Add(new MOBItem() { Id = "InflightPurchasePickPaxButton_Cancel", CurrentValue = captions.ContainsKey("InflightPurchasePickPaxButton_Back") ? captions["InflightPurchasePickPaxButton_Back"] : "Back" });
        }

        private void SetAddCardCommonCaptions(MOBInFlightPurchaseResponse response, Dictionary<string, string> captions)
        {
            response.Captions.Add(new MOBItem() { Id = "InflightPurchaseSavedCCSection_TravelersLabel", CurrentValue = captions["InflightPurchaseSavedCCSection_TravelersLabel"] });
            response.Captions.Add(new MOBItem() { Id = "InflightPurchaseCCDetails_NavigationTitle", CurrentValue = captions["InflightPurchaseCCDetails_NavigationTitle"] });
            response.Captions.Add(new MOBItem() { Id = "InflightPurchaseAddCCSection_TileLabel", CurrentValue = captions["InflightPurchaseAddCCSection_TileLabel"] });
            response.Captions.Add(new MOBItem() { Id = "InflightPurchaseCCDetails_BodyDescription", CurrentValue = captions["InflightPurchaseCCDetails_BodyDescription"] });
            if (!string.IsNullOrEmpty(response.EligibleSegments))
                response.Captions.Add(new MOBItem() { Id = "InflightPurchaseCCDetails_infoMessage", CurrentValue = captions["InflightPurchaseCCDetails_infoMessage"] });
            response.Captions.Add(new MOBItem() { Id = "InflightPurchaseCCDetails_CCDetailsEntrySectionTitle", CurrentValue = captions["InflightPurchaseCCDetails_CCDetailsEntrySectionTitle"] });
            response.Captions.Add(new MOBItem() { Id = "InflightPurchaseSavedCC_SuccessAlert", CurrentValue = captions["InflightPurchaseSavedCC_SuccessAlert"] });
        }

        private List<MOBSavedCCInflightPurchase> GetTravelersSavedCreditCardDetails(PaxWalletLookupResponse paxWalletResponse, List<MOBInflightPurchaseFlightSegments> flightSegments, ref bool isOtherTravelersHaveSavedCC)
        {
            List<MOBSavedCCInflightPurchase> savedCCList = new List<MOBSavedCCInflightPurchase>();
            if (paxWalletResponse == null)
                return savedCCList;
            string departureDate(string date)
            {
                DateTime dt;
                DateTime.TryParse(date, out dt);
                return string.Format("{0:MM/dd/yyyy}", dt);
            }
            bool doesSegmentMatch(MOBInflightPurchaseFlightSegments fs, SegmentsToPay stp) => string.Equals($"{fs.CarrierCode}{fs.FlightNumber}", stp.FlightNumber)
                                                                                                && string.Equals(fs.DepartureAirport, stp.Origin)
                                                                                                && string.Equals(departureDate(fs.DepartureDate), stp.DepartureDate);
            foreach (var segment in flightSegments)
            {
                SegmentsToPay segmentToPay = paxWalletResponse.SegmentsToPay?.FirstOrDefault(stp => doesSegmentMatch(segment, stp));
                if (segmentToPay != null)
                {
                    foreach (var paxToPay in segmentToPay.PaxToPay)
                    {
                        if (paxToPay.CreditCard != null && (string.Equals("Confirmed", paxToPay.OptInStatus, StringComparison.OrdinalIgnoreCase)
                                                                || paxToPay.IsLoyaltyProfileCard))
                        {
                            MOBInflightPurchaseTravelerInfo traveler = segment.TravelersDetails.FirstOrDefault(trvl => DoesTravelerMatch(trvl, paxToPay));
                            if (traveler != null)
                            {
                                traveler.HasSavedCreditCard = true;
                                traveler.SavedCardInfo = $"{GetCardDescription(paxToPay.CreditCard.CreditCardCode)} **{paxToPay.CreditCard.AccountNumberLastFourDigits}";
                                AddOrUpdateSavedCCList($"{segment.DepartureAirport}-{segment.ArrivalAirport}", $"{traveler.FirstName} {traveler.LastName}", traveler.SavedCardInfo, savedCCList, flightSegments.Count, segment.TravelersDetails.Count, paxToPay?.CreditCard?.PersistentToken);
                            }
                            else
                            {
                                isOtherTravelersHaveSavedCC = true;
                            }
                        }
                    }
                }
            }

            return savedCCList;
        }

        private void AddOrUpdateSavedCCList(string orignDestination, string traveler, string creditCard, List<MOBSavedCCInflightPurchase> savedCCList, int segmentsCount, int travelerCount, string persistentToken = "")
        {
            if (savedCCList != null && savedCCList.Exists(cc => cc.CardInformation.Equals(creditCard)))
            {
                var savedCC = savedCCList.Find(cc => cc.CardInformation.Equals(creditCard));
                if (!savedCC.Flights.Contains(orignDestination) && segmentsCount > 1)
                {
                    savedCC.Flights = savedCC.Flights + ", " + orignDestination;
                }
                if (!savedCC.Travelers.Contains(traveler) && travelerCount > 1)
                {
                    savedCC.Travelers = savedCC.Travelers + ", " + traveler;
                }
            }
            else
            {
                MOBSavedCCInflightPurchase cc = new MOBSavedCCInflightPurchase();
                cc.CardInformation = creditCard;
                cc.Travelers = travelerCount > 1 ? traveler : string.Empty;
                cc.Flights = segmentsCount > 1 ? orignDestination : string.Empty;
                cc.PersistentToken = persistentToken;
                savedCCList.Add(cc);
            }
        }

        private string GetCardInfo(string ccTypeDesc, string accountNumMasked)
        {
            if (!string.IsNullOrEmpty(ccTypeDesc) && !string.IsNullOrEmpty(accountNumMasked))
            {
                if (accountNumMasked.Contains(ccTypeDesc))
                {
                    return accountNumMasked;
                }
            }

            return $"{ccTypeDesc}{accountNumMasked}"; //current prod 
        }

        private async Task<Tuple<ProfileCreditCard, ProfileCreditCardResponse>> GetLoggedInMPSavedCard(MOBInFlightPurchaseRequest request, string token, string guid)
        {
            List<ProfileCreditCard> profileCreditCards = await GetSavedCCForMileaguePlusMember(request.CustomerId, request, token, request.Pnr);

            ProfileCreditCard profileCard = profileCreditCards?.FirstOrDefault(cc => cc.IsPrimary);
            if (profileCard == null)
            {
                profileCreditCards?.Sort(delegate (ProfileCreditCard card1, ProfileCreditCard card2)
                {
                    if (card1.UpdateDtmz.HasValue && card2.UpdateDtmz.HasValue)
                        return card1.UpdateDtmz.Value.CompareTo(card2.UpdateDtmz.Value);
                    return 0;
                });
                profileCard = profileCreditCards?.LastOrDefault();
            }

            Address address = null;
            if (profileCard != null)
                address = await GetProfileAddressByKey(request, request.MileagePlusNumber, profileCard.AddressKey, token, guid);

            if (address != null)
            {
                return new Tuple<ProfileCreditCard, ProfileCreditCardResponse>(profileCard, await PersistProfileCreditCardResponse(guid, request.MileagePlusNumber, profileCard, address));
            }

            return null;
        }

        private async Task<Address> GetProfileAddressByKey(MOBRequest request, string mpNumber, string addressKey, string token, string guid)
        {
            try
            {
                string url = string.Format("Address/LoyaltyId/{0}?key={1}", mpNumber, addressKey);
                string cslresponse = await _getMPNumberService.GetProfileAddressByKey(url, _headers.ContextValues.SessionId, token).ConfigureAwait(false);

                if (string.IsNullOrEmpty(cslresponse))
                {
                    return null;
                }

                CSLProfileAddressResponse response = JsonConvert.DeserializeObject<CSLProfileAddressResponse>(cslresponse);

                if (response == null)
                {
                    return null;
                }

                if (response.Errors?.Count > 0)
                {
                    string errorMsg = String.Join(", ", response.Errors.Select(x => x.Message));
                    _logger.LogWarning("GetProfileAddressByKey CSL-CallError {error} {response}", errorMsg, response);

                    return null;
                }

                CSLProfileAddress address = response.AddressData.FirstOrDefault();
                if (address == null) { return null; }
                return new Address()
                {
                    street = address.AddressLine1,
                    street2 = address.AddressLine2,
                    street3 = address.AddressLine3,
                    city = address.City,
                    postcodeZip = address.PostalCode,
                    stateProvince = address.StateCode,
                    country = address.CountryCode
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("GetProfileAddressByKey Exception {exceptionMessage} {exceptionstack}", ex.Message, JsonConvert.SerializeObject(ex));
            }

            return null;
        }

        private async Task<List<ProfileCreditCard>> GetSavedCCForMileaguePlusMember(string customerID, MOBRequest request, string token, string pnr = "")
        {
            string guid = $"{pnr}-{request.DeviceId}";
            try
            {
                string url = string.Format("creditcard/CustomerID/{0}", customerID);
                string cslresponse = await _getMPNumberService.GetSavedCCForMileaguePlusMember(url, _headers.ContextValues.SessionId, token).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(cslresponse))
                {
                    GetCreditCardDataModel response = JsonConvert.DeserializeObject<GetCreditCardDataModel>(cslresponse);


                    if (!(response != null && response.Status.Equals(United.Services.FlightShopping.Common.StatusType.Success)))
                    {
                        if (response.Errors?.Count > 0)
                        {
                            string errorMessage = String.Join(", ", response.Errors.Select(x => x.Message));
                            _logger.LogWarning("GetSavedCCForMileaguePlusMember CSL-CallError {error} {response}", errorMessage, response);

                            return null;
                        }
                    }
                    return response.CreditCards?.Where(c => IsValidCreditCard(c)).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("GetSavedCCForMileaguePlusMember Exception {exceptionMessage} {exceptionstack}", ex.Message, JsonConvert.SerializeObject(ex));
            }
            return null;
        }

        private bool IsValidCreditCard(ProfileCreditCard creditCard)
        {
            bool ok = false;

            if (!string.IsNullOrEmpty(creditCard.AddressKey))
            {
                if (creditCard.ExpYear > DateTime.Today.Year)
                {
                    ok = true;
                }
                else if (creditCard.ExpYear == DateTime.Today.Year)
                {
                    if (creditCard.ExpMonth >= DateTime.Today.Month)
                    {
                        ok = true;
                    }
                }
            }

            return ok;
        }

        private async Task<ProfileCreditCardResponse> PersistProfileCreditCardResponse(string guid, string mpNumber, ProfileCreditCard profileCreditCard, Address address)
        {
            ProfileCreditCardResponse cardResponse = new ProfileCreditCardResponse()
            {
                MileagePlusNumber = mpNumber,
                SessionId = guid,
                PrimaryCreditCard = new CreditCard()
                {
                    CreditCardCode = profileCreditCard.Code,
                    ExpiryMonthYear = profileCreditCard.ExpirationDate.ToDateTime().ToString("MMyy"),
                    AccountNumberLastFourDigits = profileCreditCard.AccountNumberLastFourDigits,
                    NameOnCard = profileCreditCard.Payor.GivenName,
                    PersistentToken = profileCreditCard.PersistentToken,
                    CurrencyCode = "USD",
                    Address = address
                }
            };
            if (!_configuration.GetValue<bool>("EnableLiftMPSavedCardVerification"))
                await _sessionHelperService.SaveSession(cardResponse, guid, new List<string> { guid, cardResponse.ObjectName }, cardResponse.ObjectName).ConfigureAwait(false);

            return cardResponse;
        }

        private async Task<PaxWalletLookupResponse> LiftMPSavedCardVerification(MOBInFlightPurchaseRequest request, string guid, string token, ProfileCreditCardResponse profileCreditCardResponse)
        {
            OptInType optInType = IsHomeScreenFlow(request.Flow) ? OptInType.PostCheckin : OptInType.Checkin;
            LookupAndSaveProfileCardRequest lookupAndSaveProfileCardRequest = new LookupAndSaveProfileCardRequest
            {
                OptInType = optInType,
                IsLoyaltyProfileCard = true,
                LoyaltyProgramMemberID = request.MileagePlusNumber,
                ChannelName = request.Application.Name,
                RecordLocator = request.Pnr,
                PaymentOrigin = optInType.ToString(),
                IPAddress = _configuration.GetValue<string>("SaveWalletStaticIpAddress"),
                CreditCard = profileCreditCardResponse.PrimaryCreditCard,
                PointOfSaleCountryCode = "US"
            };
            string departureDate(string date)
            {
                DateTime dt;
                DateTime.TryParse(date, out dt);
                return string.Format("{0:MM/dd/yyyy}", dt);
            }
            lookupAndSaveProfileCardRequest.Segments = request.FlightSegments.Where(seg => seg.IsEligible).Select(seg => new Segments
            {
                FlightNumber = $"{seg.CarrierCode}{seg.FlightNumber}",
                DepartureDate = departureDate(seg.DepartureDate),
                Origin = seg.DepartureAirport
            }).ToList();

            lookupAndSaveProfileCardRequest.PaxToPay = request.FlightSegments.First().TravelersDetails.Where(trvl => !trvl.HasSavedCreditCard).Select(trvl => new PaxToPay
            {
                FirstName = trvl.FirstName,
                LastName = trvl.LastName,
                OptInType = optInType
            }).ToList();

            return await LookupAndSaveProfileCard(request, guid, token, lookupAndSaveProfileCardRequest);
        }

        private async Task<PaxWalletLookupResponse> LookupAndSaveProfileCard(MOBInFlightPurchaseBaseRequest request, string guid, string token, SavePaxWalletRequest savePaxWalletRequest)
        {
            string url = "/LookupAndSaveProfileCard";
            string jsonRequest = JsonConvert.SerializeObject(savePaxWalletRequest);
            string cslresponse = await _getTravelersService.LookupAndSaveProfileCard(url, jsonRequest, _headers.ContextValues.SessionId, token).ConfigureAwait(false);

            if (string.IsNullOrEmpty(cslresponse))
            {
                return null;
            }

            PaxWalletLookupResponse walletLookupResponse = JsonConvert.DeserializeObject<PaxWalletLookupResponse>(cslresponse);

            return walletLookupResponse;
        }

        private void UpdateEmployeeMpNumber(MOBInFlightPurchaseRequest request, string empId, string mpNumber)
        {
            request.FlightSegments.SelectMany(seg => seg.TravelersDetails)
                .Where(trvl => empId == trvl.EmployeeId).ToList()
                .ForEach(trvl =>
                {
                    trvl.FqtvNumber = mpNumber;
                    trvl.Airline = "UA";
                });
        }

        private async Task<string> GetMPNumberByEmployeeId(MOBRequest request, string employeeId, string token, string guid)
        {
            try
            {
                string url = string.Format("MPDetailsByEmployeeId/EmployeeId/{0}", employeeId);
                MOBRequest cslRequest = new MOBRequest();

                string cslresponse = await _getMPNumberService.GetMPNumberByEmployeeId(url, _headers.ContextValues.SessionId, token).ConfigureAwait(false);


                if (string.IsNullOrEmpty(cslresponse))
                {
                    return null;
                }
                GetCreditCardDataModel response = JsonConvert.DeserializeObject<GetCreditCardDataModel>(cslresponse);

                if (response == null)
                {
                    return null;
                }

                if (response.Errors?.Count > 0)
                {
                    string errorMsg = String.Join(", ", response.Errors.Select(x => x.Message));
                    _logger.LogWarning("GetMPNumberByEmployeeId CSL-CallError {error} {response}", errorMsg, response);
                    return null;
                }

                return response.MileagePlusID;
            }
            catch (Exception ex)
            {
                _logger.LogError("GetMPNumberByEmployeeId Exception {exceptionMessage} {exceptionstack}", ex.Message, JsonConvert.SerializeObject(ex));
            }

            return null;
        }

        private bool IsMpLoggedIn(MOBInFlightPurchaseRequest request)
        {
            return request.IsLoggedIn && !string.IsNullOrWhiteSpace(request.MileagePlusNumber) && !string.IsNullOrWhiteSpace(request.HashValue);
        }

        private List<string> GetEmployeeIds(MOBInFlightPurchaseRequest request)
        {
            return request.FlightSegments.First().TravelersDetails
                .Where(trvl => !trvl.HasSavedCreditCard && !string.IsNullOrEmpty(trvl.EmployeeId))
                .Select(trvl => trvl.EmployeeId).ToList();
        }

        private void SetRTDDisplayPopupCaptions(MOBInFlightPurchaseRequest request, MOBInFlightPurchaseResponse response, Dictionary<string, string> captions)
        {
            ContactLessPaymentState state;
            if (!IsCheckinFlow(request.Flow)
                || !Enum.TryParse(response.State, out state)
                || ContactLessPaymentState.ReviewCard == state)
            {
                return;
            }

            bool ContactlessFound = response.ProductInfo.Any(s => s.ProductDetail.Contains(captions.ContainsKey("InflightPurchaseProductSection_OnboardLabel") ? captions["InflightPurchaseProductSection_OnboardLabel"] : "Onboard refreshment:"));
            if (!ContactlessFound) { return; }

            response.Captions.Add(new MOBItem() { Id = "InflightPurchaseSection_RTDAlertTitle", CurrentValue = captions.ContainsKey("InflightPurchaseSection_RTDAlertTitle") ? captions["InflightPurchaseSection_RTDAlertTitle"] : "United Airlines" });
            response.Captions.Add(new MOBItem() { Id = "InflightPurchaseSection_RTDAlertButton_Close", CurrentValue = captions.ContainsKey("InflightPurchaseSection_RTDAlertButton_Close") ? captions["InflightPurchaseSection_RTDAlertButton_Close"] : "Close" });
            if (state == ContactLessPaymentState.VerifyMPCard || state == ContactLessPaymentState.VerifyBookingCard)
            {
                response.Captions.Add(new MOBItem() { Id = "InflightPurchaseSection_RTDAlertButton_VerifyTheCard", CurrentValue = captions.ContainsKey("InflightPurchaseSection_RTDAlertButton_VerifyTheCard") ? captions["InflightPurchaseSection_RTDAlertButton_VerifyTheCard"] : "Verify card" });
                response.Captions.Add(new MOBItem() { Id = "InflightPurchaseSection_RTDVerifyCardAlertMessage", CurrentValue = captions.ContainsKey("InflightPurchaseSection_RTDVerifyCardAlertMessage") ? captions["InflightPurchaseSection_RTDVerifyCardAlertMessage"] : "You must verify your card in order to purchase onboard refreshments during your flight." });
            }
            else if (state == ContactLessPaymentState.AddCard)
            {
                response.Captions.Add(new MOBItem() { Id = "InflightPurchaseSection_RTDAlertButton_AddCreditCard", CurrentValue = captions.ContainsKey("InflightPurchaseSection_RTDAlertButton_AddCreditCard") ? captions["InflightPurchaseSection_RTDAlertButton_AddCreditCard"] : "Add credit card" });
                response.Captions.Add(new MOBItem() { Id = "InflightPurchaseSection_RTDAddCardAlertMessage", CurrentValue = captions.ContainsKey("InflightPurchaseSection_RTDAddCardAlertMessage") ? captions["InflightPurchaseSection_RTDAddCardAlertMessage"] : "We've gone contactless! Save a credit card now so you can purchase onboard refreshments during your flight." });
            }
        }

        private bool IsCheckinFlow(string flowName)
        {
            FlowType flowType;
            if (!Enum.TryParse(flowName, out flowType))
                return false;
            return flowType == FlowType.CHECKIN;
        }

        private void SetVerifyCardCaptions(MOBInFlightPurchaseResponse response, Dictionary<string, string> captions)
        {
            SetTripTipCaptions(response, captions);
            ContactLessPaymentState state = (ContactLessPaymentState)Enum.Parse(typeof(ContactLessPaymentState), response.State);
            response.Captions.Add(new MOBItem() { Id = "InflightPurchaseSavedCCSection_TravelersLabel", CurrentValue = captions["InflightPurchaseSavedCCSection_CardHolderLabel"] });
            response.Captions.Add(new MOBItem() { Id = "InflightPurchaseCCDetails_NavigationTitle", CurrentValue = captions["InflightPurchaseCCDetails_NavigationTitle"] });
            response.Captions.Add(new MOBItem() { Id = "InflightPurchase_VerifyCard_Header", CurrentValue = captions["InflightPurchase_VerifyCard_Header"] });
            if (response.FlightSegments.First().TravelersDetails.Count > 1)
                response.Captions.Add(new MOBItem() { Id = "InflightPurchase_VerifyCard_InfoAlert", CurrentValue = (state == ContactLessPaymentState.VerifyBookingCard) ? captions["InflightPurchase_VerifyBookingCard_InfoAlert"] : captions["InflightPurchase_VerifyMPCard_InfoAlert"] });
            response.Captions.Add(new MOBItem() { Id = "InflightPurchase_VerifyCard_Button_VerifyThisCard", CurrentValue = captions["InflightPurchase_VerifyCard_Button_VerifyThisCard"] });
            response.Captions.Add(new MOBItem() { Id = "InflightPurchase_VerifyCard_Button_DonotUseThisCard", CurrentValue = captions["InflightPurchase_VerifyCard_Button_DonotUseThisCard"] });
            response.Captions.Add(new MOBItem() { Id = "InflightPurchase_VerifyCard_AlertPopup_Message", CurrentValue = captions["InflightPurchase_VerifyCard_AlertPopup_Message"] });
            response.Captions.Add(new MOBItem() { Id = "InflightPurchase_VerifyCard_Alert_Button_AddDiffCard", CurrentValue = captions["InflightPurchase_VerifyCard_Alert_Button_AddDiffCard"] });
            response.Captions.Add(new MOBItem() { Id = "InflightPurchase_VerifyCard_Alert_Button_DonotAddCard", CurrentValue = captions["InflightPurchase_VerifyCard_Alert_Button_DonotAddCard"] });
        }

        private void SetTripTipCaptions(MOBInFlightPurchaseResponse response, Dictionary<string, string> captions)
        {
            ContactLessPaymentState state = (ContactLessPaymentState)Enum.Parse(typeof(ContactLessPaymentState), response.State);
            bool isHomeScreenFlow = IsHomeScreenFlow(response.Flow);

            response.Captions.Add(new MOBItem() { Id = "InflightPurchaseSection_HeaderLabel", CurrentValue = captions["InflightPurchaseSection_HeaderLabel"] });
            response.Captions.Add(new MOBItem() { Id = "InflightPurchaseSavedCCSection_CardInformationLabel", CurrentValue = captions["InflightPurchaseSavedCCSection_CardInformationLabel"] });
            response.Captions.Add(new MOBItem() { Id = "InflightPurchasePickPax_Error", CurrentValue = captions["InflightPurchasePickPax_Error"] });

            if (isHomeScreenFlow)
            {
                response.Captions.Add(new MOBItem() { Id = "InflightPurchaseCCDetails_HeaderTitle", CurrentValue = (state == ContactLessPaymentState.ReviewCard) ? captions["InflightPurchase_ReviewCard_Header"] : captions["InflightPurchase_VerifyCard_Header"] });
            }
            else
            {
                response.Captions.Add(new MOBItem() { Id = "InflightPurchaseSavedCCSection_HeaderLabel", CurrentValue = (state == ContactLessPaymentState.ReviewCard) ? captions["InflightPurchaseSavedCCSection_HeaderLabel"] : captions["InflightPurchase_VerifyCard_Header"] });
                response.Captions.Add(new MOBItem() { Id = "InflightPurchaseCCDetails_HeaderTitle", CurrentValue = (state == ContactLessPaymentState.ReviewCard) ? captions["InflightPurchase_EditCard_Button"] : captions["InflightPurchase_VerifyCard_Button"] });
            }
        }

        private string GetCardDescription(string cardType)
        {
            switch (cardType)
            {
                case "VI":
                    return "Visa";
                case "MC":
                    return "MasterCard";
                case "AX":
                    return "American Express";
                case "DS":
                    return "Discover";
                case "DC":
                    return "Diners Club Card";
                case "JC":
                    return "JCB";
                case "UP":
                    return "UnionPay";
                default:
                    return cardType;
            }
        }


        private PaxToPay GetTravelerWithBookingSavedCard(MOBInFlightPurchaseRequest request, MOBInFlightPurchaseResponse response, List<PaxToPay> paxToPayList)
        {
            List<MOBInflightPurchaseTravelerInfo> travelerInfos = response.FlightSegments.FirstOrDefault().TravelersDetails;
            PaxToPay paxToPay = paxToPayList.FirstOrDefault(ptp => travelerInfos.Exists(trvl => DoesTravelerMatch(trvl, ptp)));

            string[] pnrPaxNames = null;
            if (!string.IsNullOrEmpty(request.AllPaxNamesInPnr))
            {
                pnrPaxNames = request.AllPaxNamesInPnr.Split(',');
                if (paxToPay == null)
                    paxToPay = paxToPayList.FirstOrDefault(ptp => pnrPaxNames.Exists(name => string.Equals(name, $"{ptp.FirstName} {ptp.LastName}", StringComparison.OrdinalIgnoreCase)));
            }
            if (paxToPay == null) { return null; }

            if (travelerInfos.Count < pnrPaxNames?.Length)
            {
                pnrPaxNames.ForEach(name =>
                {
                    if (!travelerInfos.Exists(trvl => string.Equals(name, $"{trvl.FirstName} {trvl.LastName}", StringComparison.OrdinalIgnoreCase)))
                    {
                        string[] names = name.Split(' ');
                        MOBInflightPurchaseTravelerInfo travelerInfo = new MOBInflightPurchaseTravelerInfo() { FirstName = names[0], LastName = names[1] };
                        response.FlightSegments.ForEach(seg => seg.TravelersDetails.Add(travelerInfo));
                    }
                });
            }

            return paxToPay;
        }

        private bool DoesTravelerMatch(MOBInflightPurchaseTravelerInfo ti, PaxToPay ptp) => string.Equals(ti.FirstName, ptp.FirstName, StringComparison.OrdinalIgnoreCase)
                                                                                                && string.Equals(ti.LastName, ptp.LastName, StringComparison.OrdinalIgnoreCase);

        private bool IsCardValid(CreditCard creditCard)
        {
            try
            {
                DateTime expDate = DateTime.ParseExact(creditCard.ExpiryMonthYear, "MMyy", null);
                if (expDate.Year > DateTime.Today.Year
                    || (expDate.Year == DateTime.Today.Year && expDate.Month >= DateTime.Today.Month))
                {
                    return true;
                }
            }
            catch { }

            return false;
        }

        private bool DoesTravelerHasBookingSavedCard(PaxToPay pax)
        {
            return pax.OptInType == OptInType.Booking && string.Equals("Pending", pax.OptInStatus, StringComparison.OrdinalIgnoreCase);
        }

        private List<SegmentsToPay> GetSegmentsToPayMatchingWithRequestSegments(List<MOBInflightPurchaseFlightSegments> requestSegments, List<SegmentsToPay> walletLookupSegments)
        {
            bool doesMatch(SegmentsToPay wSeg) => requestSegments.Exists(rSeg =>
            {
                return string.Equals($"{rSeg.CarrierCode}{rSeg.FlightNumber}", wSeg.FlightNumber)
                    && string.Equals(rSeg.DepartureAirport, wSeg.Origin)
                    && (DateTime.TryParse(rSeg.DepartureDate, out DateTime rSegDate)
                        && DateTime.TryParse(wSeg.DepartureDate, out DateTime wSegDate)
                        && rSegDate.Equals(wSegDate));
            });

            return walletLookupSegments?.Where(wSeg => doesMatch(wSeg))?.ToList();
        }

        private async Task<PaxWalletLookupResponse> GetTravelersSavedCCDetails(MOBRequest request, string token, string pnr)
        {
            string guid = $"{pnr}-{request.DeviceId}";
            var lookupWalletRequest = new PaxWalletLookupRequest();
            lookupWalletRequest.RecordLocator = pnr;
            // string url = "/PaxWalletLookup";

            //EnableContactLessSaveToProfileForPayment
            string url = "/PaxWalletLookupDetails";

            string cslLookupWalletRequest = JsonConvert.SerializeObject(lookupWalletRequest);
            string cslresponse = await _getTravelersService.GetTravelersSavedCCDetails(url, cslLookupWalletRequest, _headers.ContextValues.SessionId, token).ConfigureAwait(false);

            if (string.IsNullOrEmpty(cslresponse))
            {
                return null;
            }

            PaxWalletLookupResponse walletLookupResponse = JsonConvert.DeserializeObject<PaxWalletLookupResponse>(cslresponse);

            return walletLookupResponse;
        }
        private MOBItem GetCCImageUrl()
        {
            MOBItem item = new MOBItem
            {
                Id = "InflightPurchaseCC_ImageUrl",
                CurrentValue = _configuration.GetValue<string>("HomeScreenAssetBaseUrl")
            };
            return item;
        }
        private void UpdateMOBInFlightPurchaseResponse(MOBInFlightPurchaseResponse response, List<MOBInflightPurchaseFlightSegments> flightSegments, Dictionary<string, string> captions)
        {
            response.IsEligibleToAddNewCCForInflightPurchase = true;
            response.HideInflightPurchaseSection = false;
            response.FlightSegments = flightSegments;
            response.AllowPaxSelection = flightSegments.First().TravelersDetails.Count > 1;
            response.Captions.Add(GetCCImageUrl());
            if (flightSegments.Any(seg => !seg.IsEligible))
            {
                response.EligibleSegments = string.Join(",", flightSegments.Where(seg => seg.IsEligible).Select(s => $"{s.DepartureAirport}-{s.ArrivalAirport}"));
                response.Captions.Add(new MOBItem() { Id = "InflightPurchase_For", CurrentValue = captions["InflightPurchase_For_Label"] });
                if (flightSegments.Any(seg => "UA" != seg.CarrierCode && !seg.IsDBA))
                {
                    response.AttentionMessages.Add(captions["InflightPurchase_AttentionMessage_OA"]);
                }
            }
        }

        private void SetContactLesStatusCaptions(MOBInFlightPurchaseResponse response, Dictionary<string, string> captions, System.Collections.ObjectModel.Collection<United.Service.Presentation.ProductModel.ProductSegment> productSegments, string programName)
        {
            if (captions == null)
                return;
            string[] programNames = programName.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (productSegments != null && productSegments.Any() && programNames.Any())
            {
                bool isAnycontactLessSegments = productSegments.Where(p => p.Characteristic.Any(c => c.Code.Equals(programNames[0]) && c.Value.Equals("True"))).Any();
                if (isAnycontactLessSegments)
                {
                    response.Captions.Add(new MOBItem() { Id = "ContactLessStatus", CurrentValue = "true" });
                }
                else
                {
                    response.Captions.Add(new MOBItem() { Id = "ContactLessStatus", CurrentValue = "false" });
                }
            }
        }

        private void SetContactLessAODCaptions(MOBInFlightPurchaseResponse response, Dictionary<string, string> captions)
        {
            if (captions == null)
                return;
            response.Captions.Add(new MOBItem() { Id = "InflightPurchase_Product_Header", CurrentValue = captions.ContainsKey("InflightPurchaseProductSection_HeaderLabel") ? captions["InflightPurchaseProductSection_HeaderLabel"] : "Contactless payments apply to:" });
            response.Captions.Add(new MOBItem() { Id = "InflightPurchase_Modal_Header", CurrentValue = captions.ContainsKey("InflightPurchase_Modal_Header") ? captions["InflightPurchase_Modal_Header"] : "What are contactless payments?" });
            response.Captions.Add(new MOBItem() { Id = "InflightPurchase_Modal_Content", CurrentValue = captions.ContainsKey("InflightPurchase_Modal_Content") ? captions["InflightPurchase_Modal_Content"] : "Save a debit or credit card to the United app for contactless payments. United agents and flight attendants will be able to automatically charge this card for qualifying transactions. Contactless payments can be used for certain airport transaction, such as bag check and seat upgrades. Or, they can be used to purchase onboard snacks and refreshments if available on your flight." });
            response.Captions.Add(new MOBItem() { Id = "InflightPurchase_Modal_Button_Close", CurrentValue = captions["InflightPurchaseSection_RTDAlertButton_Close"] });
        }

        private List<MOBInflightProductInfo> GetFlightProductInfo(Collection<ProductSegment> productSegments, string programName, string onboardlabel, string aodlabel)
        {
            List<MOBInflightProductInfo> productInfo = new List<MOBInflightProductInfo>();
            string[] programNames = programName.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (productSegments != null && productSegments.Any() && programNames.Any())
            {
                string flightdetails = string.Join(",", productSegments.Where(p => p.Characteristic.Any(c => c.Code.Equals(programNames[0]) && c.Value.Equals("True"))).Select(p => $"{p.DepartureAirport.IATACode}-{p.ArrivalAirport.IATACode}"));
                if (!string.IsNullOrEmpty(flightdetails))
                {
                    productInfo.Add(new MOBInflightProductInfo
                    {
                        Flights = flightdetails,
                        ProductDetail = onboardlabel
                    });
                }
                flightdetails = string.Join(",", productSegments.Where(p => p.Characteristic.Any(c => c.Code.Equals(programNames[1]) && c.Value.Equals("True"))).Select(p => $"{p.DepartureAirport.IATACode}-{p.ArrivalAirport.IATACode}"));
                if (!string.IsNullOrEmpty(flightdetails))
                {
                    productInfo.Add(new MOBInflightProductInfo
                    {
                        Flights = flightdetails,
                        ProductDetail = aodlabel
                    });
                }
            }

            return productInfo;
        }

        private async Task<Collection<ProductSegment>> GetInflightPurchaseEligibility(MOBInFlightPurchaseRequest request, string token, string guid, string programName)
        {

            United.Service.Presentation.ProductRequestModel.ProductEligibilityRequest eligibilityRequest = new United.Service.Presentation.ProductRequestModel.ProductEligibilityRequest();
            eligibilityRequest.Filters = new System.Collections.ObjectModel.Collection<United.Service.Presentation.ProductRequestModel.ProductFilter>()
            {
                new United.Service.Presentation.ProductRequestModel.ProductFilter()
                {
                    ProductCode = "PEC"
                }
            };
            eligibilityRequest.Requestor = new United.Service.Presentation.CommonModel.Requestor()
            {
                ChannelID = _configuration.GetValue<string>("MerchandizeOffersCSLServiceChannelID"),
                ChannelName = _configuration.GetValue<string>("MerchandizeOffersCSLServiceChannelName")
            };

            int segNum = 0;
            string departureDateTime(string date)
            {
                DateTime dt;
                DateTime.TryParse(date, out dt);
                return dt != null ? dt.ToString() : date;
            }

            string[] programNames = programName.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var characteristics = new System.Collections.ObjectModel.Collection<United.Service.Presentation.CommonModel.Characteristic>();
            programNames?.ForEach(p =>
            {
                characteristics.Add(new Service.Presentation.CommonModel.Characteristic() { Code = "Program", Value = p });
            });

            eligibilityRequest.FlightSegments = request.FlightSegments.Select(seg => new United.Service.Presentation.ProductModel.ProductSegment()
            {
                SegmentNumber = ++segNum,
                ClassOfService = seg.ClassOfService,
                OperatingAirlineCode = seg.CarrierCode,
                DepartureDateTime = departureDateTime(seg.DepartureDate),
                DepartureAirport = new United.Service.Presentation.CommonModel.AirportModel.Airport() { IATACode = seg.DepartureAirport },
                ArrivalAirport = new United.Service.Presentation.CommonModel.AirportModel.Airport() { IATACode = seg.ArrivalAirport },
                Characteristic = characteristics

            }).ToCollection();

            string jsonRequest = JsonConvert.SerializeObject(eligibilityRequest);

            var cslresponse = await _purchaseMerchandizingService.GetInflightPurchaseEligibility(token, jsonRequest, _headers.ContextValues.SessionId).ConfigureAwait(false);
            var response = JsonConvert.DeserializeObject<ProductEligibilityResponse>(cslresponse);

            if (response == null)
            {
                return null;
            }

            if ((response?.FlightSegments?.Count ?? 0) == 0)
            {
                return null;
            }

            if (response.Response.Error?.Count > 0)
            {
                string errorMsg = String.Join(", ", response.Response.Error.Select(x => x.Text));
                _logger.LogWarning("GetInflightPurchaseEligibility CSL-CallError {response} {ErrorMessage}", response, errorMsg);
                return null;
            }

            return response.FlightSegments;
        }

        private async Task<Dictionary<string, string>> GetSDLCaptions(string token)
        {
            KeyValuePairContent kvpContent = await GetSDLKeyValuePairContentByPageName(token, "inflight-sdlcontent");
            if (kvpContent == null)
            {
                _logger.LogError("GetSDLCaptions-SDLkeyvaluepair is null {SDLKeyValuePair}", kvpContent);
            }
            return kvpContent.content.labels.ToDictionary(lbl => lbl.content.Key, lbl => lbl.content.Value);
        }
        private async Task<KeyValuePairContent> GetSDLKeyValuePairContentByPageName(string token, string docNameConfigEntry)
        {
            var response = await _inflightDynamoDB.GetInflightContent<SDLKeyValuePairContentResponse>(docNameConfigEntry);
            if (response != null) { return response.content.FirstOrDefault(); }
            string cslresponse = await _getSDLKeyValuePairContentService.GetSDLKeyValuePairContentByPageName(string.Empty, _headers.ContextValues.SessionId, token).ConfigureAwait(false);
            response = JsonConvert.DeserializeObject<SDLKeyValuePairContentResponse>(cslresponse);
            await _dynamoDBService.SaveRecords("cuw-cache-content","cache_content",docNameConfigEntry, response, "");
            return response.content?.FirstOrDefault();
        }

        private void PopulateMOBInFlightPurchaseResponse(MOBInFlightPurchaseResponse response, MOBInFlightPurchaseBaseRequest request)
        {
            response.HideInflightPurchaseSection = true;
            response.SavedCreditcardsInfo = new List<MOBSavedCCInflightPurchase>();
            response.Captions = new List<MOBItem>();
            response.FlightSegments = new List<MOBInflightPurchaseFlightSegments>();
            response.Pnr = request.Pnr;
            response.Flow = request.Flow;
            response.AttentionMessages = new List<string>();
        }
    }
}
