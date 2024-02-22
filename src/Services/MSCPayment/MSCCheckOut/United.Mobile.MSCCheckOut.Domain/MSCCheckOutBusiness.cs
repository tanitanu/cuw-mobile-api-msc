using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using United.Common.Helper;
using United.Common.Helper.MSCPayment.Interfaces;
using United.Definition;
using United.Mobile.DataAccess.Common;
using United.Mobile.DataAccess.MSCPayment.Interfaces;
using United.Mobile.DataAccess.Product.Interfaces;
using United.Mobile.Model.Common;
using United.Persist.Definition.Shopping;
using United.Utility.Helper;
using MOBCPTraveler = United.Definition.MOBCPTraveler;

namespace United.Mobile.MSCCheckOut.Domain
{
    public class MSCCheckOutBusiness : IMSCCheckOutBusiness
    {
        private readonly ICacheLog<MSCCheckOutBusiness> _logger;
        private readonly IConfiguration _configuration;
        private readonly ISessionHelperService _sessionHelperService;
        private readonly ICheckoutUtiliy _checkOutUtility;
        private readonly IShoppingCartUtility _shoppingCartUtility;
        private readonly ILegalDocumentsForTitlesService _legalDocumentsForTitlesService;
        private readonly IMerchandizingCSLService _merchandizingCSLService;
        private readonly IFeatureToggles _featureToggles;

        public MSCCheckOutBusiness(ICacheLog<MSCCheckOutBusiness> logger
            , ICheckoutUtiliy checkOutUtility
            , IConfiguration configuration
            , ISessionHelperService sessionHelperService
            , IShoppingCartUtility shoppingCartUtility
            , ILegalDocumentsForTitlesService legalDocumentsForTitlesService
            , IMerchandizingCSLService merchandizingCSLService
            , IFeatureToggles featureToggles)
        {
            _logger = logger;
            _configuration = configuration;
            _checkOutUtility = checkOutUtility;
            _sessionHelperService = sessionHelperService;
            _shoppingCartUtility = shoppingCartUtility;
            _legalDocumentsForTitlesService = legalDocumentsForTitlesService;
            _merchandizingCSLService = merchandizingCSLService;
            _featureToggles = featureToggles;
            ConfigUtility.UtilityInitialize(_configuration);
        }

        public async Task<MOBCheckOutResponse> CheckOut(MOBCheckOutRequest request)
        {
            Session session = null;
            if (!string.IsNullOrEmpty(request.SessionId))
            {
                session = await _checkOutUtility.GetValidateSession(request.SessionId, false, true);
                session.Flow = request.Flow;
            }

            MOBCheckOutResponse response = new MOBCheckOutResponse();
            MOBShoppingCart shoppingCart = new MOBShoppingCart();
            if (_configuration.GetValue<bool>("EnableApplePayLogging"))
            {
                await _sessionHelperService.SaveSession<MOBCheckOutRequest>(request, request.SessionId, new List<string> { request.SessionId, request.GetType().FullName}, request.GetType().FullName);
            }
            response = await _checkOutUtility.CheckOut(request);

            if (request.Flow.ToUpper() == FlowType.BOOKING.ToString() && response.Reservation != null && response.ShoppingCart != null)
            {
                response.Reservation.IsBookingCommonFOPEnabled = response.Reservation.IsBookingCommonFOPEnabled;
                shoppingCart = response.ShoppingCart;
                shoppingCart = await _shoppingCartUtility.ReservationToShoppingCart_DataMigration(response.Reservation, shoppingCart, request, true, session);
                shoppingCart.Flow = request.Flow;
                await _sessionHelperService.SaveSession<MOBShoppingCart>(shoppingCart, request.SessionId, new List<string> { request.SessionId, shoppingCart.ObjectName }, shoppingCart.ObjectName);
                response.ShoppingCart = shoppingCart;
                //IOS - Eplus older than 41 displays Total price on Registration details page instead of GrandTotal
                if (response.Reservation?.TravelOptions?.Any(f => f.Key == "EFS") == true
                        && _configuration.GetValue<bool>("AFSRFareLockIssue")
                        && request.Application.Id == 1
                        && GeneralHelper.IsApplicationVersionGreaterorEqual(request.Application.Id, request.Application.Version.Major, "", _configuration.GetValue<string>("iPhoneAFSRFareLockIssueVersion")) == false)
                {
                    if (response.Reservation != null && !string.IsNullOrEmpty(response.Reservation.RecordLocator)
                            && response.Reservation.TravelOptions?.Any(f => f.Key == "FareLock") == true)
                    {
                        if (response.Reservation.Prices != null)
                        {
                            var grandTotal = response.Reservation.Prices.FirstOrDefault(g => g.DisplayType == "GRAND TOTAL");
                            if (grandTotal != null)
                            {
                                response.Reservation.Prices.Where(x => x.DisplayType == "TOTAL").ToList().ForEach(p =>
                                {
                                    if (p != null)
                                    {
                                        p.DisplayValue = grandTotal.DisplayValue;
                                        p.FormattedDisplayValue = grandTotal.FormattedDisplayValue;
                                        p.Value = grandTotal.Value;
                                    }
                                });
                            }
                        }
                    }
                }

                #region TripInsuranceV2
                // We will notify Merch if the user declines the trip insurance offer
                if (request != null &&
                    request.IsTPIOfferDeclinedByUser &&
                    await _featureToggles.IsEnabledTripInsuranceV2(request.Application.Id, request.Application.Version.Major, session.CatalogItems).ConfigureAwait(false)
                    )
                {
                    _checkOutUtility.NotifyTPIOfferDeclined(request.SessionId);
                }                
                #endregion

            }
            else if (request.Flow.ToUpper() == FlowType.RESHOP.ToString() && response.Reservation != null && response.ShoppingCart != null)
            {
                response.Reservation.IsReshopCommonFOPEnabled = response.Reservation.IsReshopCommonFOPEnabled;
                shoppingCart = response.ShoppingCart;
                shoppingCart = await _shoppingCartUtility.ReservationToShoppingCart_DataMigration(response.Reservation, shoppingCart, request);

                shoppingCart.Flow = request.Flow;
                await _sessionHelperService.SaveSession<MOBShoppingCart>(shoppingCart, request.SessionId, new List<string> { request.SessionId, shoppingCart.ObjectName }, shoppingCart.ObjectName);
                response.ShoppingCart = shoppingCart;
            }
            ////From MFOP we will not consider RefundPrice --- Updating at the root level for MFOP we are not adding RefundPrice
            //if(ConfigUtility.EnableMFOP(request.Application.Id, request.Application.Version.Major))
            //{
            //    response.Reservation.Prices.Remove(response.Reservation.Prices.FirstOrDefault(x => x.DisplayType.ToUpper() == "REFUNDPRICE"));
            //}
            if (_configuration.GetValue<bool>("DisableFFCSummaryPriceChange") == false && ConfigUtility.IsFFCSummaryUpdated(request.Application.Id, request.Application.Version.Major))
            {
                if (response.Reservation != null && (request.Flow.ToUpper() == FlowType.BOOKING.ToString() || request.Flow.ToUpper() == FlowType.RESHOP.ToString()))
                {
                    // MOBILE-22205 MOBILE-22565 Per the story GRAND TOTAL should not // turn offf below FFC Summary for MFOP as we have to show total due as same as ETC
                    if (response.Reservation.Prices != null && response.Reservation.Prices.Count > 0 && response.Reservation.Prices.Where(a => a.DisplayType == "FFC").Any() && !ConfigUtility.EnableMFOP(request.Application.Id, request.Application.Version.Major))
                    {
                        var grandtotal = response.Reservation.Prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "GRAND TOTAL");
                        var total = response.Reservation.Prices.FirstOrDefault(p => p.DisplayType.ToUpper() == "FFC");
                        if (grandtotal != null && total != null)
                        {
                            grandtotal.Value += total.Value;
                            grandtotal.FormattedDisplayValue = grandtotal.Value.ToString("C2", CultureInfo.CurrentCulture); ;
                            grandtotal.DisplayValue = grandtotal.FormattedDisplayValue;
                        }
                    }
                }
            }


            //MB-1146 Send email to Accessibility Desk for high-touch service requests
            if (!ConfigUtility.IsViewResFlowCheckOut(request.Flow) &&  _configuration.GetValue<bool>("EnableEmailSentToAccessibilityDeskForSSRs"))
            {
                List<LogEntry> SSRLogContext = new List<LogEntry>();
                await Task.Factory.StartNew(() => FireAndForgetSSREmailSend(request, response, session));
            }

            if (response.Reservation != null)
                response.Reservation.CartId = request.CartId;
            if (request.Flow.ToUpper() == FlowType.BOOKING.ToString() && response.Reservation != null  && response.Reservation.ShopReservationInfo2 !=null && !string.IsNullOrEmpty(response.Reservation.ShopReservationInfo2.CartRefId))
            {
                response.Reservation.ShopReservationInfo2.CartRefId = string.Empty;
                if(response.Reservation.ShopReservationInfo2.CartRefIdContentMsg !=null)
                    response.Reservation.ShopReservationInfo2.CartRefIdContentMsg = null;
            }
            return response;
        }

        public async Task<List<MOBItem>> GetMPPINPWDTitleMessages(string titleList)
        {
            List<MOBItem> messages = new List<MOBItem>();
            List<MOBLegalDocument> docs = await _legalDocumentsForTitlesService.GetNewLegalDocumentsForTitles(titleList, "trans0", true).ConfigureAwait(false);
            if (docs != null && docs.Count > 0)
            {
                foreach (var doc in docs)
                {
                    MOBItem item = new MOBItem();
                    item.Id = doc.Title;
                    item.CurrentValue = doc.LegalDocument;
                    messages.Add(item);
                }
            }
            return messages;
        }

        private void FireAndForgetSSREmailSend(MOBCheckOutRequest request, MOBCheckOutResponse response, Session session)
        {
            try
            {
                // Send only when departure is not within 168 hours
                if (response.Reservation != null && response.Reservation.ShopReservationInfo2 != null && !response.Reservation.ShopReservationInfo2.PurchaseToTravelTimeIsWithinSevenDays
                    && response.ShoppingCart != null && response.ShoppingCart.SCTravelers != null && response.Reservation.Trips.Any() && response.Reservation.Trips[0].FlattenedFlights.Any()
                    && response.Reservation.Trips[0].FlattenedFlights[0].Flights[0].DepartDate != null
                    && response.ShoppingCart.FormofPaymentDetails != null)
                {
                    var listOfHighTouchRequestCodes = _configuration.GetValue<string>("EmailAgent_HighTouchSSRs").Split('|');

                    List<MOBCPTraveler> SSRTravelers = response.ShoppingCart.SCTravelers.Where(t => t.SelectedSpecialNeeds.Where(_ => listOfHighTouchRequestCodes.Contains(_.Code) || (_.Code == "ESAN" && _.Value == "5")).Any()).ToList();
                    string TextSender = "";
                    if (SSRTravelers.Any())
                    {
                        if (response.ShoppingCart.FormofPaymentDetails.EmailAddress != null)
                        {
                            TextSender = response.ShoppingCart.FormofPaymentDetails.EmailAddress;
                        }
                        string DepartureDate = response.Reservation.Trips[0].FlattenedFlights[0].Flights[0].DepartDate.Split(',').Length >= 2 ? response.Reservation.Trips[0].FlattenedFlights[0].Flights[0].DepartDate.Split(',')[1] : response.Reservation.Trips[0].FlattenedFlights[0].Flights[0].DepartDate;
                        string TextSubject = string.Format(_configuration.GetValue<string>("EmailSubjectForSSRs"), response.ShoppingCart.PointofSale, "English", DepartureDate);
                        string TextBody = "";
                        foreach (var traveler in SSRTravelers)
                        {
                            foreach (var selectedssr in traveler.SelectedSpecialNeeds)
                            {
                                if (selectedssr.Code == "ESAN" && selectedssr.Value == "5")
                                {
                                    TextBody += string.Format(_configuration.GetValue<string>("EmailBodyForSSRs_1"), "SVAN", response.RecordLocator, traveler.LastName, traveler.FirstName, Environment.NewLine);
                                }
                                else if (listOfHighTouchRequestCodes.Contains(selectedssr.Code))
                                {
                                    TextBody += string.Format(_configuration.GetValue<string>("EmailBodyForSSRs_1"), selectedssr.Code, response.RecordLocator, traveler.LastName, traveler.FirstName, Environment.NewLine);
                                }
                            }
                        }
                        string phonenumber = "not provided";
                        if (response.ShoppingCart.FormofPaymentDetails.Phone != null)
                        {
                            if (response.ShoppingCart.FormofPaymentDetails.Phone.AreaNumber != null && response.ShoppingCart.FormofPaymentDetails.Phone.AreaNumber != "" && response.ShoppingCart.FormofPaymentDetails.Phone.PhoneNumber != null && response.ShoppingCart.FormofPaymentDetails.Phone.PhoneNumber.Length >= 7)
                            {
                                phonenumber = string.Format("{0}-{1}-{2}", response.ShoppingCart.FormofPaymentDetails.Phone.AreaNumber, response.ShoppingCart.FormofPaymentDetails.Phone.PhoneNumber.Substring(0, 3), response.ShoppingCart.FormofPaymentDetails.Phone.PhoneNumber.Substring(3));
                            }
                            else
                            {
                                phonenumber = response.ShoppingCart.FormofPaymentDetails.Phone.PhoneNumber;
                            }
                            if (phonenumber != "not provided" && response.ShoppingCart.FormofPaymentDetails.Phone.CountryPhoneNumber != null && response.ShoppingCart.FormofPaymentDetails.Phone.CountryPhoneNumber != "")
                            {
                                phonenumber = response.ShoppingCart.FormofPaymentDetails.Phone.CountryPhoneNumber + "-" + phonenumber;
                            }
                        }
                        TextBody += string.Format(_configuration.GetValue<string>("EmailBodyForSSRs_2"), phonenumber, Environment.NewLine, TextSender, Environment.NewLine);
                        SendEmail(TextSubject, TextBody, TextSender, _configuration.GetValue<string>("EmailRecipientForSSRs"), request.Application.Id, request.SessionId, request.Application.Version.Major, request.DeviceId);
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.LogError("FireAndForgetSSREmailSend Error {message} {StackTrace} and {session}", ex.Message, ex.StackTrace, request.SessionId);
            }
            finally
            {
                //logger.Write(LogEntries);
                _logger.LogInformation("FireAndForgetSSREmailSend Request{request} sessionId{sessionId}", JsonConvert.SerializeObject(request), request.SessionId);
            }
        }

        private void SendEmail(string subject, string emailBody, string emailFrom, string emailTo, int appId, string sessionId, string major, string deviceId)
        {
            try
            {
                using (SmtpClient smtp = new SmtpClient(_configuration.GetValue<string>("EmailServer")))
                {
                    MailMessage mail = new MailMessage();
                    mail.From = new MailAddress(emailFrom);
                    mail.IsBodyHtml = false;
                    mail.Subject = subject;
                    mail.Body = emailBody;
                    mail.To.Add(emailTo != null ? emailTo : "");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                    _logger.LogInformation("SendEmail {message} with {sessionId}", "EmailSentSuccessfully", sessionId);
                }
            }
            catch (SmtpFailedRecipientException exe)
            {
                _logger.LogError("SendEmail Error {message} {StackTrace} and {session}", exe.Message, exe.StackTrace, sessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError("SendEmail Error {message} {StackTrace} and {session}", ex.Message, ex.StackTrace, sessionId);
            }
        }

    }
}
