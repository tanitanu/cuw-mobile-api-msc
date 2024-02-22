using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using United.Common.Helper.MSCPayment.Interfaces;
using United.Definition;
using United.Definition.Pcu;
using United.Definition.Shopping;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.DynamoDB;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Persist.Definition.Merchandizing;
using United.Persist.Definition.Pcu;
using United.Persist.Definition.Shopping;
using United.Service.Presentation.ProductModel;
using United.Service.Presentation.ProductResponseModel;
using United.Service.Presentation.SegmentModel;
using United.Services.FlightShopping.Common.Cart;
using United.Services.FlightShopping.Common.Extensions;
using United.Utility.Helper;
using Characteristic = United.Service.Presentation.CommonModel.Characteristic;
using Reservation = United.Service.Presentation.ReservationModel.Reservation;
using United.Mobile.Model.Common;
namespace United.Common.Helper.MSCPayment
{
    public class MSCPremiumCabinUpgrade
    {
        private const string CountryCode = "US";
        private const string LangCode = "en-US";
        private const string ProductCodePcu = "PCU";
        private const string CartKeyPcuBookingPath = "ConfirmationPCU";
        public string RecordLocator;
        public bool IsValidOffer;

        private string _sessionId;
        private string _token;
        private Reservation _cslReservation;
        private bool _isPostBookingPurchase;
        public string CartId;
        private MOBRequest _mobRequest;
        private readonly IConfiguration _configuration;
        private readonly ILegalDocumentsForTitlesService _legalDocumentsForTitlesService;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly IMSCPkDispenserPublicKey _mSCPkDispenserPublicKey;
        private readonly IFlightShoppingProductsService _flightShoppingProductsService;


        public DisplayCartRequest ProductRequest;
        public ProductOffer MerchProductOffer;
        public MOBPremiumCabinUpgrade CabinUpgradeOffer { get; set; }

        public MSCPremiumCabinUpgrade(IConfiguration configuration
            , ILegalDocumentsForTitlesService legalDocumentsForTitlesService
            , IMSCPkDispenserPublicKey mSCPkDispenserPublicKey
            , ISessionHelperService sessionHelperService
            , IFlightShoppingProductsService flightShoppingProductsService
            )
        {
            _configuration = configuration;
            _legalDocumentsForTitlesService = legalDocumentsForTitlesService;
            _mSCPkDispenserPublicKey = mSCPkDispenserPublicKey;
            _sessionHelperService = sessionHelperService;
            _flightShoppingProductsService = flightShoppingProductsService;
        }

        public void Initialization(string recordLocator, string sessionId, string cartId, MOBRequest mobRequest
            , Reservation cslReservation
            )
        {
            if(!_configuration.GetValue<bool>("DisableManageResChanges23C"))
                ClearPcuState(sessionId);
            this.RecordLocator = recordLocator;
            this._sessionId = sessionId;
            this._mobRequest = mobRequest;
            this.CartId = cartId;
            this._isPostBookingPurchase = !string.IsNullOrEmpty(cartId);
            this._cslReservation = _configuration.GetValue<bool>("RevertPassingReservationToGetProductsPCU") ? null : cslReservation;
        }

        public async System.Threading.Tasks.Task<MSCPremiumCabinUpgrade> BuildPremiumCabinUpgrade()
        {
            if (!this.IsValidOffer) return this;

            var premiumCabinUpgrade = new MOBPremiumCabinUpgrade();
            premiumCabinUpgrade.CartId = CartId;
            premiumCabinUpgrade.PcuOptions = await BuildPcuOptions().ConfigureAwait(false);
            premiumCabinUpgrade.ProductCode = MerchProductOffer.Offers[0].ProductInformation.ProductDetails[0].Product.Code;
            premiumCabinUpgrade.ProductName = MerchProductOffer.Offers[0].ProductInformation.ProductDetails[0].Product.DisplayName;
            if (premiumCabinUpgrade.PcuOptions == null ||
                premiumCabinUpgrade.PcuOptions.EligibleTravelers == null ||
                premiumCabinUpgrade.PcuOptions.EligibleSegments == null ||
                !premiumCabinUpgrade.PcuOptions.EligibleSegments.Any()) return this;

            var amount = premiumCabinUpgrade.PcuOptions.EligibleSegments.Where(s => !string.IsNullOrEmpty(s.FormattedPrice)).Min(s => s.Price);
            if (!(amount > 0)) return null;

            premiumCabinUpgrade.OfferTile = await BuildOfferTile(amount, "PCU_OfferTile").ConfigureAwait(false);
            premiumCabinUpgrade.Captions = await BuildPcuCaptions(premiumCabinUpgrade.PcuOptions.EligibleTravelers.Count > 1).ConfigureAwait(false);
            premiumCabinUpgrade.MobileCmsContentMessages = await GetTermsAndConditions("trans0").ConfigureAwait(false);
            this.CabinUpgradeOffer = premiumCabinUpgrade;
            return this;
        }

        public async System.Threading.Tasks.Task<MSCPremiumCabinUpgrade> GetpkDispenserPublicKey(List<United.Mobile.Model.Common.MOBItem> catalogItems=null)
        {
            if (!this.IsValidOffer) return this;
            this.CabinUpgradeOffer.PkDispenserPublicKey = await _mSCPkDispenserPublicKey.GetCachedOrNewpkDispenserPublicKey(this._mobRequest.Application.Id, this._mobRequest.Application.Version.Major, this._mobRequest.DeviceId, _sessionId, _token, catalogItems).ConfigureAwait(false);
            return this;
        }

        public async System.Threading.Tasks.Task SaveState()
        {
            if (!IsValidOffer) return;

            var pcuState = new PcuState();
            pcuState.CartId = CartId;
            pcuState.EligibleSegments = CabinUpgradeOffer.PcuOptions.EligibleSegments;
            pcuState.RecordLocator = RecordLocator;
            pcuState.LastName = MerchProductOffer.Travelers[0].Surname;
            pcuState.NumberOfPax = MerchProductOffer.Travelers.Count;
            pcuState.IsPostBookingPurchase = _isPostBookingPurchase;
            pcuState.PremiumCabinUpgradeOfferDetail = CabinUpgradeOffer;
            await _sessionHelperService.SaveSession(pcuState, _sessionId, new List<string> { _sessionId, pcuState.ObjectName }, pcuState.ObjectName).ConfigureAwait(false);
        }
        private async void ClearPcuState(string sessionId)
        {
            var pcuState = new PcuState();
            await _sessionHelperService.SaveSession(pcuState, sessionId, new List<string> { sessionId, pcuState.ObjectName }, pcuState.ObjectName).ConfigureAwait(false);
        }

        public async System.Threading.Tasks.Task<List<MOBMobileCMSContentMessages>> GetTermsAndConditions(string transactionId)
        {
            var cmsContentMessages = new List<MOBMobileCMSContentMessages>();
            var docKeys =  "PCU_TnC" ;
            var docs = await _legalDocumentsForTitlesService.GetNewLegalDocumentsForTitles(docKeys,transactionId, true).ConfigureAwait(false);
            if (docs != null && docs.Any())
            {
                foreach (var doc in docs)
                {
                    var cmsContentMessage = new MOBMobileCMSContentMessages();
                    cmsContentMessage.ContentFull = doc.LegalDocument;
                    cmsContentMessage.Title = doc.Title;
                    cmsContentMessages.Add(cmsContentMessage);
                }
            }

            return cmsContentMessages;
        }

        public async  System.Threading.Tasks.Task<MSCPremiumCabinUpgrade> GetTokenFromSession(string sessionId)
        {
            var session = new Session();
            session = await _sessionHelperService.GetSession<Session>(sessionId, session.ObjectName, new List<string> { sessionId, session.ObjectName }).ConfigureAwait(false);
            this._token = session.Token;
            return this;
        }

        private async System.Threading.Tasks.Task<List<MOBItem>> BuildPcuCaptions(bool isMultiPax)
        {
            var captions = await GetMPPINPWDTitleMessages( "PCU_UpgradeFlight_Captions" ).ConfigureAwait(false);

            if (_configuration.GetValue<bool>("EnablePcuMultipleUpgradeOptions"))
            {
                if (!isMultiPax && captions != null)
                {
                    captions.RemoveWhere(c => c != null && c.Id == "MultiPaxNoteText");
                }
            }
            return captions;
        }

        public async System.Threading.Tasks.Task<List<MOBItem>> GetMPPINPWDTitleMessages(string titleList)
        {
            List<MOBItem> messages = new List<MOBItem>();
           var docs = await _legalDocumentsForTitlesService.GetNewLegalDocumentsForTitles(titleList,"trans0",true).ConfigureAwait(false);
            if (docs != null && docs.Count > 0)
            {
                foreach (MOBLegalDocument doc in docs)
                {
                    MOBItem item = new MOBItem();
                    item.Id = doc.Title;
                    item.CurrentValue = doc.LegalDocument;
                    messages.Add(item);
                }
            }
            return messages;
        }

        internal async System.Threading.Tasks.Task<MOBOfferTile> BuildOfferTile(double offerPrice, string captionKey)
        {
            if (offerPrice <= 0 || string.IsNullOrWhiteSpace(captionKey))
                return null;

            var offerTileCaptions = await GetCaptions(captionKey).ConfigureAwait(false);
            var offerTile = new MOBOfferTile();
            offerTile.Price = (decimal)Math.Round(offerPrice);
            foreach (var caption in offerTileCaptions)
            {
                switch (caption.Id.ToUpper())
                {
                    case "TITLE":
                        offerTile.Title = caption.CurrentValue;
                        break;
                    case "TEXT1":
                        offerTile.Text1 = caption.CurrentValue;
                        break;
                    case "TEXT2":
                        offerTile.Text2 = caption.CurrentValue;
                        break;
                    case "TEXT3":
                        offerTile.Text3 = caption.CurrentValue;
                        break;
                    case "CURRENCYCODE":
                        offerTile.CurrencyCode = caption.CurrentValue;
                        break;
                }
            }

            return offerTile;
        }

        public  virtual async System.Threading.Tasks.Task<MOBPcuOptions> BuildPcuOptions()
        {

            var subProducts = MerchProductOffer.Offers[0].ProductInformation.ProductDetails[0].Product.SubProducts;
            var flightSegments = MerchProductOffer.FlightSegments ?? MerchProductOffer.Solutions[0].ODOptions.SelectMany(o => o.FlightSegments).ToCollection();

            MOBPcuOptions pcuOptions = new MOBPcuOptions();
            pcuOptions.EligibleTravelers = MerchProductOffer.Travelers.Select(t => string.Format("{0}, {1}", t.Surname, t.GivenName)).ToList();
            pcuOptions.EligibleSegments = new List<MOBPcuSegment>();
            pcuOptions.CurrencyCode = "$";
            var noOfTravelers = MerchProductOffer.Travelers.Count;
            var pcuSegments = new List<MOBPcuSegment>();
            foreach (var flightSegment in flightSegments)
            {
                if (!ValidFlighSegmentData(flightSegment)) continue;
                var offers = subProducts.Where(sp => IsEligibleSegment(flightSegment.ID, sp));

                var segment = new MOBPcuSegment
                {
                    FlightDescription = string.Format("{0}{1} - {2} to {3}", flightSegment.MarketedFlightSegment[0].MarketingAirlineCode, flightSegment.MarketedFlightSegment[0].FlightNumber, flightSegment.DepartureAirport.IATACode, flightSegment.ArrivalAirport.IATACode),
                    SegmentNumber = flightSegment.SegmentNumber,
                    Origin = flightSegment.DepartureAirport.IATACode.ToUpper(),
                    Destination = flightSegment.ArrivalAirport.IATACode.ToUpper(),
                    FlightNumber = flightSegment.MarketedFlightSegment[0].FlightNumber,
                    SegmentDescription = string.Format("{0} - {1}", flightSegment.DepartureAirport.IATACode.ToUpper(), flightSegment.ArrivalAirport.IATACode.ToUpper()),
                    NoOfTravelersText = noOfTravelers + (noOfTravelers == 1 ? " Traveler /" : " Travelers /"),
                    UpgradeOptions = BuildUpgradeOptions(noOfTravelers, flightSegment, offers)
                };
                PopulateMoreFieldsToSupportOlderVerionsOfApps(ref segment);
                pcuSegments.Add(segment);
            }
            if (_configuration.GetValue<bool>("EnablePcuMultipleUpgradeOptions"))
            {
                pcuOptions.CompareOptions = await BuildCompareOptions(pcuSegments).ConfigureAwait(false);
            }
            if (pcuSegments.Any(s => s.Price > 0))
            {
                if (EnablePCU_NotAvailableVersion(_mobRequest.Application.Id, _mobRequest.Application.Version.Major))
                {
                    pcuOptions.EligibleSegments.AddRange(pcuSegments);
                }
                else
                {
                    pcuSegments.RemoveAll(s => s.Price <= 0);
                    pcuOptions.EligibleSegments.AddRange(pcuSegments);
                }
            }

            return pcuOptions;
        }

        private static void PopulateMoreFieldsToSupportOlderVerionsOfApps(ref MOBPcuSegment segment)
        {
            //to support older clients
            if (segment.UpgradeOptions != null && segment.UpgradeOptions.Any())
            {
                var lowestPricedUpgradeOption = segment.UpgradeOptions.Aggregate((currentItem, u) => u.Price != 0 && u.Price < currentItem.Price ? u : currentItem);
                segment.CabinDescription = lowestPricedUpgradeOption.CabinDescriptionForPaymentPage;
                segment.UpgradeDescription = lowestPricedUpgradeOption.UpgradeOptionDescription;
                segment.Price = lowestPricedUpgradeOption.Price;
                segment.FormattedPrice = string.Format("${0}", lowestPricedUpgradeOption.Price);
                segment.TotalPriceForAllTravelers = lowestPricedUpgradeOption.TotalPriceForAllTravelers;
                segment.ProductIds = lowestPricedUpgradeOption.ProductIds;
            }
        }

        private async System.Threading.Tasks.Task<List<MOBPcuUpgradeOptionInfo>> BuildCompareOptions(List<MOBPcuSegment> pcuSegments)
        {
            if (pcuSegments == null || !pcuSegments.Any() || !pcuSegments.Any(s => s.UpgradeOptions != null && s.UpgradeOptions.Any()))
                return null;

            var upgradeCabinsOffered = pcuSegments.SelectMany(s => GetOfferedCabinDescriptions(s.UpgradeOptions)).Distinct().ToList();

            var compareOptionsCaptions = await GetCaptions("PCU_COMPAREOPTIONS").ConfigureAwait(false);
            if (compareOptionsCaptions == null || !compareOptionsCaptions.Any())
                return null;

            compareOptionsCaptions.RemoveWhere(c => !upgradeCabinsOffered.Contains(c.Id, StringComparer.OrdinalIgnoreCase));
            var compareOptions = compareOptionsCaptions.Select(c => AddUpgradeOptionInfo(c.CurrentValue)).ToList();
            return compareOptions;
        }

        private MOBPcuUpgradeOptionInfo AddUpgradeOptionInfo(string dbContent)
        {
            if (string.IsNullOrEmpty(dbContent))
                return null;

            var details = dbContent.Split('|');
            if (details == null || details.Length < 4)
                return null;

            return new MOBPcuUpgradeOptionInfo()
            {
                Product = details[0],
                Header = details[1],
                Body = details[2],
                ImageUrl = details[3]
            };
        }

        private IEnumerable<string> GetOfferedCabinDescriptions(List<MOBPcuUpgradeOption> upgradeOptions)
        {
            if (upgradeOptions == null || !upgradeOptions.Any())
                return new List<string>();

            return upgradeOptions.Select(u => u != null && u.UpgradeOptionDescription != null ? u.UpgradeOptionDescription.Trim().Replace("®", "").Replace("℠", "").Replace(" ", "") : string.Empty);
        }

        private async System.Threading.Tasks.Task<List<MOBItem>> GetCaptions(string key)
        {
            return !string.IsNullOrEmpty(key) ? await GetCaptions( key , true).ConfigureAwait(false) : null;
        }

        private async System.Threading.Tasks.Task<List<MOBItem>> GetCaptions(string keyList, bool isTnC)
        {
            var docs = await _legalDocumentsForTitlesService.GetNewLegalDocumentsForTitles(keyList, "trans0",true).ConfigureAwait(false);
            if (docs == null || !docs.Any()) return null;

            var captions = new List<MOBItem>();

            captions.AddRange(
                docs.Select(doc => new MOBItem
                {
                    Id = doc.Title,
                    CurrentValue = doc.LegalDocument
                }));
            return captions;
        }

        public bool EnablePCU_NotAvailableVersion(int appId, string appVersion)
        {
            return _configuration.GetValue<bool>("EnablePCU")
                    && GeneralHelper.IsApplicationVersionGreater(appId, appVersion, "AndroidPCU_NotAvailableVersion", "iPhonePCU_NotAvailableVersion", "", "", true, _configuration);
        }

        private List<MOBPcuUpgradeOption> BuildUpgradeOptions(int noOfTravelers, ProductFlightSegment flightSegment, IEnumerable<SubProduct> offers)
        {
            if (noOfTravelers <= 0 || flightSegment == null || offers == null || !offers.Any())
                return null;

            var options = offers.Where(sp => IsEligibleSegment(flightSegment.ID, sp))
                                .Select(o => BuildUpgradeOption(noOfTravelers, flightSegment.ID, o)).ToList();
            if (!options.Any())
                return null;

            options.RemoveAll(o => o.Price <= 0);
            if (!options.Any())
                return null;

            options = options.OrderBy(o => o.Price).ToList();
            options.Add(new MOBPcuUpgradeOption
            {
                UpgradeOptionDescription = "I don't want to upgrade this flight"
            });

            return options;
        }

        private MOBPcuUpgradeOption BuildUpgradeOption(int noOfTravelers, string segmentId, SubProduct subProduct)
        {
            var amount = GetAmount(subProduct.Prices[0]);
            var cabinName = ConfigUtility.GetFormattedCabinName(subProduct.Descriptions[0]);
            return new MOBPcuUpgradeOption()
            {
                OptionId = segmentId + subProduct.Code,
                UpgradeOptionDescription = cabinName,
                CabinDescriptionForPaymentPage = string.Format("{0}:", cabinName),
                FormattedPrice = TopHelper.FormatAmountForDisplay(amount.ToString(), new CultureInfo("en-US")),
                Price = amount,
                TotalPriceForAllTravelers = noOfTravelers * amount,
                ProductIds = subProduct.Prices.Select(p => p.ID).ToList()
            };
        }

        private double GetAmount(ProductPriceOption priceOption)
        {
            if (priceOption == null ||
                priceOption.PaymentOptions == null ||
                !priceOption.PaymentOptions.Any() ||
                priceOption.PaymentOptions[0] == null ||
                priceOption.PaymentOptions[0].PriceComponents == null ||
                !priceOption.PaymentOptions[0].PriceComponents.Any() ||
                priceOption.PaymentOptions[0].PriceComponents[0].Price == null ||
                priceOption.PaymentOptions[0].PriceComponents[0].Price.Totals == null ||
                !priceOption.PaymentOptions[0].PriceComponents[0].Price.Totals.Any())
                return 0;

            return priceOption.PaymentOptions[0].PriceComponents[0].Price.Totals[0].Amount;
        }

        private bool IsEligibleSegment(string segId, SubProduct subProduct)
        {
            if (string.IsNullOrEmpty(segId) ||
                subProduct == null ||
                subProduct.Extension == null ||
                subProduct.Extension.AdditionalExtensions == null ||
                !subProduct.Extension.AdditionalExtensions.Any() ||
                subProduct.Extension.AdditionalExtensions[0].Assocatiation == null ||
                subProduct.Extension.AdditionalExtensions[0].Assocatiation.SegmentRefIDs == null ||
                !subProduct.Extension.AdditionalExtensions[0].Assocatiation.SegmentRefIDs.Any() ||
                subProduct.Prices == null || !subProduct.Prices.Any())
                return false;

            return segId == subProduct.Extension.AdditionalExtensions[0].Assocatiation.SegmentRefIDs[0];
        }

        private bool ValidFlighSegmentData(ProductFlightSegment flightSegment)
        {
            return !(flightSegment == null ||
                     flightSegment.DepartureAirport == null || string.IsNullOrEmpty(flightSegment.DepartureAirport.IATACode) ||
                     flightSegment.ArrivalAirport == null || string.IsNullOrEmpty(flightSegment.ArrivalAirport.IATACode) ||
                     flightSegment.MarketedFlightSegment == null || !flightSegment.MarketedFlightSegment.Any() ||
                     string.IsNullOrEmpty(flightSegment.MarketedFlightSegment[0].MarketingAirlineCode) || string.IsNullOrEmpty(flightSegment.MarketedFlightSegment[0].FlightNumber));
        }

        public async System.Threading.Tasks.Task<MSCPremiumCabinUpgrade> GetOffer()
        {
            //var productResponse = _flightShopping.GetProducts(ProductRequest, _mobRequest, _sessionId, GetLogActionName());
            var productResponse = await GetProducts(ProductRequest, _mobRequest, _sessionId, GetLogActionName()).ConfigureAwait(false);
            if (productResponse == null)
                return this;

            this.CartId = productResponse.CartId;
            this.MerchProductOffer = productResponse.MerchProductOffer;
            return this;
        }

        public async System.Threading.Tasks.Task<DisplayCartResponse> GetProducts(DisplayCartRequest request, MOBRequest mobRequest, string sessionId, string logAction)
        {
            var session = new Session();
            session = await _sessionHelperService.GetSession<Session>(sessionId,session.ObjectName,new List<string> { sessionId, session.ObjectName }).ConfigureAwait(false);
            
            var jsonRequest = JsonConvert.SerializeObject(request);
            var jsonResponse = await _flightShoppingProductsService.GetProducts(session.Token,sessionId, jsonRequest, mobRequest.TransactionId).ConfigureAwait(false);
            var response = jsonResponse==null ? null :
                           JsonConvert.DeserializeObject<DisplayCartResponse>(jsonResponse);
            CheckServiceErrorsForGetProducts(response);
            //Adding PCU offer to persist
            var productOffer = new GetOffers();
            productOffer = ObjectToObjectCasting<United.Persist.Definition.Merchandizing.GetOffers, United.Service.Presentation.ProductResponseModel.ProductOffer>(response.MerchProductOffer);
            await PersistAncillaryProducts(sessionId, productOffer).ConfigureAwait(false);
            return response;
        }

        private void RemoveProductOfferIfAlreadyExists(GetOffers productOffers, string product)
        {
            if (product == "Bundle")
            {
                //If it is bundles remove all the bundle products in the offers.
                if (productOffers?.Offers != null)
                {
                    var products = productOffers.Offers.First()
                          .ProductInformation
                          .ProductDetails
                          .Where(productDetail =>
                                     productDetail?.Product?.SubProducts != null
                                     && productDetail.Product.SubProducts.Any(subProduct => subProduct.GroupCode == "BE"));//Get all products with groupCode as BE (This would be Be only for bundle products)
                    if (products != null)
                    {
                        foreach (var bundleProduct in products.ToList())
                        {
                            productOffers.Offers.First().ProductInformation.ProductDetails.Remove(bundleProduct);
                        }

                    }
                }
            }
        }

        private async System.Threading.Tasks.Task PersistAncillaryProducts(string sessionId, United.Persist.Definition.Merchandizing.GetOffers productOffer, bool IsCCEDynamicOffer = false, String product = "")
        {

            await System.Threading.Tasks.Task.Run(async() =>
            {
                var persistedProductOffers = new United.Persist.Definition.Merchandizing.GetOffers();
                persistedProductOffers = await _sessionHelperService.GetSession<United.Persist.Definition.Merchandizing.GetOffers>(sessionId, persistedProductOffers.ObjectName.ToString(), new List<string> { sessionId, persistedProductOffers.ObjectName.ToString() }).ConfigureAwait(false);
                if (_configuration.GetValue<bool>("EnableOmniCartReleaseCandidateTwoChanges_Bundles") && !string.IsNullOrEmpty(product))
                {
                    //Remove the Existing offer if we are making the dynamicOffer call multiple times with the same session
                    RemoveProductOfferIfAlreadyExists(persistedProductOffers, product);
                }

                if (persistedProductOffers != null && persistedProductOffers.Offers?.Count > 0)
                {
                    if (!_configuration.GetValue<bool>("DisablePostBookingPurchaseFailureFix"))//Flightsegments need to be updated when ever we get an offer for the product.
                    {
                        persistedProductOffers.FlightSegments = productOffer.FlightSegments;
                        persistedProductOffers.Travelers = productOffer.Travelers;
                        persistedProductOffers.Solutions = productOffer.Solutions;
                        persistedProductOffers.Response = productOffer.Response;
                        persistedProductOffers.Requester = productOffer.Requester;
                    }

                    if (_configuration.GetValue<bool>("EnableOmniCartReleaseCandidateTwoChanges_Bundles") && !string.IsNullOrEmpty(product) && productOffer.Offers.FirstOrDefault().ProductInformation.ProductDetails != null && productOffer.Offers.FirstOrDefault().ProductInformation.ProductDetails.Count > 0)
                    {
                        foreach (var productDetails in productOffer.Offers.FirstOrDefault().ProductInformation.ProductDetails)
                        {
                            persistedProductOffers.Offers.FirstOrDefault().ProductInformation.ProductDetails.Add(productDetails);
                        }
                    }
                    else
                    {
                        persistedProductOffers.Offers.FirstOrDefault().ProductInformation.ProductDetails.Add(productOffer.Offers.FirstOrDefault().ProductInformation.ProductDetails.FirstOrDefault());
                    }
                }
                else
                {
                    persistedProductOffers = productOffer;
                }
                if (_configuration.GetValue<bool>("EnableOmniCartReleaseCandidateTwoChanges_Bundles"))
                {
                    if (IsCCEDynamicOffer)
                    {
                        if (productOffer.Characteristics == null)
                        {
                            productOffer.Characteristics = new Collection<Characteristic>();
                        }
                        if (persistedProductOffers != null && !persistedProductOffers.Characteristics.Any(characteristic => characteristic.Code == "IsEnabledThroughCCE"))
                        {
                            productOffer.Characteristics.Add(new Characteristic { Code = "IsEnabledThroughCCE", Value = "True" });
                        }
                    }
                    else// Need to remove this characteristics when IsCCEDynamicOffer==false ,As this is the same method we use for saving the postbooking products (PBS and PCU) at that we shouldnt send this characteristis as we are going to flightshoppig.
                    {
                        if (persistedProductOffers != null && persistedProductOffers.Characteristics?.Any(characteristic => characteristic.Code == "IsEnabledThroughCCE") == true)
                        {
                            persistedProductOffers.Characteristics.Remove(persistedProductOffers.Characteristics.First(characteristic => characteristic.Code == "IsEnabledThroughCCE"));
                        }
                    }
                }
               await _sessionHelperService.SaveSession<United.Persist.Definition.Merchandizing.GetOffers>(persistedProductOffers, sessionId, new List<string> { sessionId, persistedProductOffers.ObjectName.ToString() },persistedProductOffers.ObjectName.ToString() ).ConfigureAwait(false);
            });
        }

        private T ObjectToObjectCasting<T, R>(R request)
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

        private void CheckServiceErrorsForGetProducts(DisplayCartResponse response)
        {
            if (response == null) throw new Exception("response is null");
            if (response.Errors == null || !response.Errors.Any()) return;

            var errorMessage = response.Errors.Aggregate(string.Empty, (current, e) => current + " " + e.Message);
            throw new MOBUnitedException(errorMessage);
        }

        private string GetLogActionName()
        {
            return _isPostBookingPurchase ? "PCUPostBooking" : "PremiumCabinUpgrade";
        }

        public async Task<MSCPremiumCabinUpgrade> ValidateOfferResponse()
        {
            IsValidOffer = !string.IsNullOrEmpty(CartId) &&
                            MerchProductOffer != null &&
                            MerchProductOffer.Offers != null &&
                            MerchProductOffer.Offers.Any() &&
                            MerchProductOffer.Offers[0].ProductInformation != null &&
                            MerchProductOffer.Offers[0].ProductInformation.ProductDetails != null &&
                            MerchProductOffer.Offers[0].ProductInformation.ProductDetails.Any() &&
                            MerchProductOffer.Offers[0].ProductInformation.ProductDetails[0].Product != null &&
                            MerchProductOffer.Offers[0].ProductInformation.ProductDetails[0].Product.SubProducts != null &&
                            MerchProductOffer.Offers[0].ProductInformation.ProductDetails[0].Product.SubProducts.Any() &&
                            MerchProductOffer.Offers[0].ProductInformation.ProductDetails[0].Product.SubProducts.Any(SubProductWithPrice()) &&
                            MerchProductOffer.Travelers != null &&
                            MerchProductOffer.Travelers.Any() &&
                            MerchProductOffer.Response != null &&
                            MerchProductOffer.Response.RecordLocator != null &&
                            MerchProductOffer.Response.RecordLocator.Equals(RecordLocator, StringComparison.InvariantCultureIgnoreCase) &&
                            (MerchProductOffer.Solutions != null &&
                            MerchProductOffer.Solutions.Any() &&
                            MerchProductOffer.Solutions[0].ODOptions != null &&
                            MerchProductOffer.Solutions[0].ODOptions.Any() ||
                            MerchProductOffer.FlightSegments != null &&
                            MerchProductOffer.FlightSegments.Any());

            return await Task.FromResult(this);
        }

        private Func<SubProduct, bool> SubProductWithPrice()
        {
            return sp => sp != null && sp.Prices != null &&
                   sp.Prices.Any(p => p != null &&
                                 p.PaymentOptions != null &&
                                 p.PaymentOptions.Any(po => po != null &&
                                                      po.PriceComponents != null &&
                                                      po.PriceComponents.Any(pc => pc != null && pc.Price != null && pc.Price.Totals != null && pc.Price.Totals.Any() && pc.Price.Totals[0].Amount > 0)));
        }

        public MSCPremiumCabinUpgrade BuildOfferRequest()
        {

            this.ProductRequest = new DisplayCartRequest
            {
                CartId = this.CartId,
                CartKey = this._isPostBookingPurchase ? CartKeyPcuBookingPath : "",
                CountryCode = CountryCode,
                LangCode = LangCode,
                Characteristics = CharacteristicsWithDeviceType(_mobRequest.Application.Id),
                ProductCodes = new List<string> { ProductCodePcu },
                Reservation = GetCslReservation()
            };

            return this;
        }

        private Reservation GetCslReservation()
        {
            return _cslReservation ?? new Reservation { ConfirmationID = this.RecordLocator };
        }

        private Collection<Characteristic> CharacteristicsWithDeviceType(int id)
        {
            string deviceType;
            switch (id)
            {
                case 1:
                    deviceType = "iOS";
                    break;
                case 2:
                    deviceType = "Android";
                    break;
                default:
                    return null;
            }

            return new Collection<Characteristic>
            {
                new Characteristic {Code = "RTD_DeviceType", Value = deviceType},
            };
        }
    }
}
