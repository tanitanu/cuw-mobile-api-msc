using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using United.Definition.Shopping;
using United.Mobile.Model.Common;
namespace United.Definition.MPPINPWD
{
    [Serializable()]
    public class MOBMPPINPWDValidateRequest : MOBRequest
    {
        private string mileagePlusNumber = string.Empty;
        private int customerID;
        private string passWord = string.Empty;
        private string sessionID = string.Empty;
        private bool signInWithTouchID;
        private MOBMPSignInPath mpSignInPath;
        private string hashValue = string.Empty;
        private bool rememberDevice;
        
        public MOBMPPINPWDValidateRequest()
            : base()
        {
        }

        public string MileagePlusNumber
        {
            get
            {
                return this.mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public int CustomerID
        {
            get
            {
                return this.customerID;
            }
            set
            {
                this.customerID = value;
            }
        }

        public string PassWord
        {
            get
            {
                return this.passWord;
            }
            set
            {
                this.passWord = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        public string SessionID
        {
            get
            {
                return this.sessionID;
            }
            set
            {
                this.sessionID = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        public MOBMPSignInPath MPSignInPath
        {
            get
            {
                return this.mpSignInPath;
            }
            set
            {
                this.mpSignInPath = value;
            }
        }

        public bool SignInWithTouchID
        {
            get { return this.signInWithTouchID; }
            set { this.signInWithTouchID = value; }
        }

        public string HashValue
        {
            get
            {
                return this.hashValue;
            }
            set
            {
                this.hashValue = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public bool RememberDevice
        {
            get { return rememberDevice; }
            set { rememberDevice = value; }
        }
    }

    [Serializable]
    public enum MOBMPSignInPath
    {
        [EnumMember(Value = "None")]
        None,
        [EnumMember(Value = "MyAccountPath")]
        MyAccountPath,
        [EnumMember(Value = "AwardTBookingPath")]
        AwardTBookingPath,
        [EnumMember(Value = "RevenueBookingPath")]
        RevenueBookingPath,
        [EnumMember(Value = "CorporateBookingPath")]
        CorporateBookingPath,
        [EnumMember(Value = "CorporateChangePath")]
        CorporateChangePath
    }
    [Serializable()]
    public class MOBMPPINPWDValidateResponse : MOBResponse
    {
        public MOBMPPINPWDValidateResponse()
            : base()
        {
        }

        private MOBSHOPResponseStatusItem responseStatusItem;
        public MOBSHOPResponseStatusItem ResponseStatusItem
        {
            get { return responseStatusItem; }
            set { responseStatusItem = value; }
        }

        private MOBMPAccountValidationRequest request;
        public MOBMPAccountValidationRequest Request
        {
            get
            {
                return this.request;
            }
            set
            {
                this.request = value;
            }
        }

        private MOBMPAccountValidation accountValidation;
        public MOBMPAccountValidation AccountValidation
        {
            get
            {
                return this.accountValidation;
            }
            set
            {
                this.accountValidation = value;
            }
        }

        private bool securityUpdate;
        public bool SecurityUpdate
        {
            get { return this.securityUpdate; }
            set { this.securityUpdate = value; }
        }

        private MOBMPPINPWDSecurityUpdateDetails mpSecurityUpdateDetails;
        public MOBMPPINPWDSecurityUpdateDetails MPSecurityUpdateDetails
        {
            get
            {
                return this.mpSecurityUpdateDetails;
            }
            set
            {
                this.mpSecurityUpdateDetails = value;
            }
        }

        private string sessionID = string.Empty;
        public string SessionID
        {
            get
            {
                return this.sessionID;
            }
            set
            {
                this.sessionID = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        private List<MOBItem> landingPageMessages;
        public List<MOBItem> LandingPageMessages
        {
            get
            {
                return this.landingPageMessages;
            }
            set
            {
                this.landingPageMessages = value;
            }
        }

        private MOBMPAccountSummary opAccountSummary;
        private MOBProfile profile;
        private bool isUASubscriptionsAvailable;
        private MOBUASubscriptions uaSubscriptions;
        public MOBMPAccountSummary OPAccountSummary
        {
            get
            {
                return this.opAccountSummary;
            }
            set
            {
                this.opAccountSummary = value;
            }
        }

        public MOBProfile Profile
        {
            get
            {
                return this.profile;
            }
            set
            {
                this.profile = value;
            }
        }

        public bool ISUASubscriptionsAvailable
        {
            get
            {
                return this.isUASubscriptionsAvailable;
            }
            set
            {
                this.isUASubscriptionsAvailable = value;
            }
        }

        public MOBUASubscriptions UASubscriptions
        {
            get
            {
                return this.uaSubscriptions;
            }
            set
            {
                this.uaSubscriptions = value;
            }
        }

        private bool isExpertModeEnabled;
        public bool IsExpertModeEnabled { get { return this.isExpertModeEnabled; } set { this.isExpertModeEnabled = value; } }

        private string employeeId = string.Empty;
        public string EmployeeId
        {
            get
            {
                return this.employeeId;
            }
            set
            {
                this.employeeId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        private string displayEmployeeId = string.Empty;
        public string DisplayEmployeeId
        {
            get
            {
                return this.displayEmployeeId;
            }
            set
            {
                this.displayEmployeeId = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        private Emp.MOBEmpTravelTypeResponse empTravelTypeResponse;
        public Emp.MOBEmpTravelTypeResponse EmpTravelTypeResponse
        {
            get
            {
                return this.empTravelTypeResponse;
            }
            set
            {
                this.empTravelTypeResponse = value;
            }
        }

        private Corporate.MOBCorporateTravelType corporateEligibleTravelType;

        public Corporate.MOBCorporateTravelType CorporateEligibleTravelType
        {
            get { return corporateEligibleTravelType; }
            set { corporateEligibleTravelType = value; }
        }
       
        private MOBMPTFARememberMeFlags rememberMEFlags;
        public MOBMPTFARememberMeFlags RememberMEFlags
        {
            get
            {
                if (rememberMEFlags == null)
                {
                    rememberMEFlags = new MOBMPTFARememberMeFlags();
                }
                return this.rememberMEFlags;
            }
            set
            {
                this.rememberMEFlags = value;
            }
        }

        private bool showContinueAsGuestButton;
        public bool ShowContinueAsGuestButton
        {
            get
            {
                return showContinueAsGuestButton;
            }
            set { showContinueAsGuestButton = value; }
        }

        private MOBCPCustomerMetrics customerMetrics;
        public MOBCPCustomerMetrics CustomerMetrics
        {
            get { return customerMetrics; }
            set { customerMetrics = value; }
        }
    }

    [Serializable()]
    public class MOBMPPINPWDSecurityUpdateDetails
    {
        public MOBMPPINPWDSecurityUpdateDetails()
            : base()
        {
        }
        private MOBMPSecurityUpdatePath mpSecurityPath;
        private int daysToCompleteSecurityUpdate;
        private bool passwordOnlyAllowed; // passwordOnlyAllowed says 
        private bool updateLaterAllowed;
        private bool forceSignOut; // This is to force sign out when update later is disabled as its time to update the Security Data and will be forced to update data to move forward (As of now here too other than Revenue Booking)
        public bool ForceSignOut
        {
            get { return this.forceSignOut; }
            set { this.forceSignOut = value; }
        }
        private List<MOBMPSecurityUpdatePath> mpSecurityPathList;
        private MOBMPPINPWDSecurityItems securityItems;
        private bool isPinPwdAutoSignIn = false;

        //The "daysToCompleteSecurityUpdate" property communicates how many days the user has to complete the security updates
        //The "passwordOnlyAllowed" property communicates that only their password can be used to sign-in.  If this property is true then check the value entered by the user in the PIN/Password input field.  If the user entered a 4 digit value then log them out and display an error message.
        //The "updateLaterAllowed" property communicates whether the user can perform the security updates later or not.


        public MOBMPPINPWDSecurityItems SecurityItems
        {
            get { return securityItems; }
            set { securityItems = value; }
        }

        public MOBMPSecurityUpdatePath MPSecurityPath
        {
            get
            {
                return this.mpSecurityPath;
            }
            set
            {
                this.mpSecurityPath = value;
            }
        }

        public int DaysToCompleteSecurityUpdate
        {
            get
            {
                return this.daysToCompleteSecurityUpdate;
            }
            set
            {
                this.daysToCompleteSecurityUpdate = value;
            }
        }

        public bool PasswordOnlyAllowed
        {
            get { return this.passwordOnlyAllowed; }
            set { this.passwordOnlyAllowed = value; }
        }

        public bool UpdateLaterAllowed
        {
            get { return this.updateLaterAllowed; }
            set { this.updateLaterAllowed = value; }
        }

        public List<MOBMPSecurityUpdatePath> MPSecurityPathList
        {
            get
            {
                return this.mpSecurityPathList;
            }
            set
            {
                this.mpSecurityPathList = value;
            }
        }

        private List<MOBItem> securityUpdateMessages;
        public List<MOBItem> SecurityUpdateMessages
        {
            get
            {
                return this.securityUpdateMessages;
            }
            set
            {
                this.securityUpdateMessages = value;
            }
        }

        //TFS Backlog Defect #27502 - PINPWD AutoSignIn
        public bool IsPinPwdAutoSignIn
        {
            get { return this.isPinPwdAutoSignIn; }
            set { this.isPinPwdAutoSignIn = value; }
        }
        

    }

    [Serializable]
    public enum MOBMPSecurityUpdatePath
    {
        [EnumMember(Value = "None")]
        None,
        [EnumMember(Value = "VerifyPrimaryEmail")]
        VerifyPrimaryEmail,
        [EnumMember(Value = "NoPrimayEmailExist")]
        NoPrimayEmailExist,
        [EnumMember(Value = "UpdatePassword")]
        UpdatePassword,
        [EnumMember(Value = "UpdateSecurityQuestions")]
        UpdateSecurityQuestions,
        [EnumMember(Value = "SignInBackWithNewPassWord")]
        SignInBackWithNewPassWord,
        [EnumMember(Value = "ForgotMileagePlusNumber")]
        ForgotMileagePlusNumber,
        [EnumMember(Value = "ForgotMPPassWord")]
        ForgotMPPassWord,
        [EnumMember(Value = "ValidateSecurityQuestions")]
        ValidateSecurityQuestions,
        [EnumMember(Value = "IncorrectSecurityQuestion")]
        IncorrectSecurityQuestion,
        [EnumMember(Value = "IncorrectUserDetails")]
        IncorrectUserDetails,
        [EnumMember(Value = "UnableToResetOnline")]
        UnableToResetOnline,
        [EnumMember(Value ="AccountLocked")]
        AccountLocked,
        [EnumMember(Value = "ValidateTFASecurityQuestions")]
        ValidateTFASecurityQuestions,
        [EnumMember(Value = "TFAAccountLocked")]
        TFAAccountLocked,
        [EnumMember(Value = "TFAForgotPasswordEmail")]
        TFAForgotPasswordEmail,
        [EnumMember(Value = "TFAAccountResetEmail")]
        TFAAccountResetEmail,
        [EnumMember(Value = "TFAInvalidAccountResetEmail")]
        TFAInvalidAccountResetEmail


        // when forceSignOut is true for update later is disabled at ValidateMPSignIn() as its time to update the Security Data and will be forced to update data to move forward (As of now here too other than Revenue Booking) - 
        // - than after update password success then need to force client to sign in back with re-enter the new password and sign in.
        //“VerifyPrimaryEmail” means need to verify saved primary email
        //“NoPrimayEmailExist” means no primary email exists
        //“UpdatePassword” means a valid password does not exist
        //“UpdateSecurityQuestions” means the 5 security questions and answers do not exist
    }

    [Serializable()]
    public class MOBMPPINPWDSecurityItems
    {
        public MOBMPPINPWDSecurityItems()
            : base()
        {
            needQuestionsCount = Convert.ToInt32(ConfigurationManager.AppSettings["NumberOfSecurityQuestionsNeedatPINPWDUpdate"].ToString());
        }
        private string primaryEmailAddress;
        public string PrimaryEmailAddress
        {
            get
            {
                return this.primaryEmailAddress;
            }
            set
            {
                this.primaryEmailAddress = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        private int needQuestionsCount;
        public int NeedQuestionsCount
        {
            get
            {
                return this.needQuestionsCount;
            }
            set
            {
                this.needQuestionsCount = value;
            }
        }

        private List<Securityquestion> securityQuestions;
        public List<Securityquestion> AllSecurityQuestions
        {
            get { return securityQuestions; }
            set { securityQuestions = value; }
        }

        private List<Securityquestion> savedSecurityQuestions;
        public List<Securityquestion> SavedSecurityQuestions
        {
            get { return savedSecurityQuestions; }
            set { savedSecurityQuestions = value; }
        }

        private string updatedPassword;
        public string UpdatedPassword
        {
            get
            {
                return this.updatedPassword;
            }
            set
            {
                this.updatedPassword = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

    }

    [Serializable()]
    public class Securityquestion
    {
        private int questionID;
        public int QuestionId
        {
            get { return questionID; }
            set { questionID = value; }
        }

        public string questionKey;
        public string QuestionKey
        {
            get { return questionKey; }
            set { questionKey = value; }
        }

        private string questionText;
        public string QuestionText
        {
            get { return questionText; }
            set { questionText = value; }
        }

        private bool used;
        public bool Used
        {
            get { return used; }
            set { used = value; }
        }


        private List<Answer> answers;
        public List<Answer> Answers
        {
            get { return answers; }
            set { answers = value; }
        }

    }

    [Serializable()]
    public class Answer
    {
        private int answerID;
        public int AnswerId
        {
            get { return answerID; }
            set { answerID = value; }
        }
        public string answerKey;
        public string AnswerKey
        {
            get { return answerKey; }
            set { answerKey = value; }
        }

        public string questionKey;
        public string QuestionKey
        {
            get { return questionKey; }
            set { questionKey = value; }
        }
        private int questionID;
        public int QuestionId
        {
            get { return questionID; }
            set { questionID = value; }
        }
        private string answerText;
        public string AnswerText
        {
            get { return answerText; }
            set { answerText = value; }
        }
    }

    [Serializable]
    public class MOBMPSecurityUpdate
    {
        public MOBMPSecurityUpdate()
            : base()
        {
        }
        private List<MOBMPSecurityUpdatePath> mpSecurityPathList;
        private int daysToCompleteSecurityUpdate;
        private bool passwordOnlyAllowed;
        private bool updateLaterAllowed;

        public List<MOBMPSecurityUpdatePath> MPSecurityPathList
        {
            get
            {
                return this.mpSecurityPathList;
            }
            set
            {
                this.mpSecurityPathList = value;
            }
        }

        public int DaysToCompleteSecurityUpdate
        {
            get
            {
                return this.daysToCompleteSecurityUpdate;
            }
            set
            {
                this.daysToCompleteSecurityUpdate = value;
            }
        }

        public bool PasswordOnlyAllowed
        {
            get { return this.passwordOnlyAllowed; }
            set { this.passwordOnlyAllowed = value; }
        }

        public bool UpdateLaterAllowed
        {
            get { return this.updateLaterAllowed; }
            set { this.updateLaterAllowed = value; }
        }

    }

    [Serializable()]
    public class MOBMPSecurityUpdateRequest : MOBRequest
    {

        private string sessionID = string.Empty;
        private string mileagePlusNumber = string.Empty;
        private int customerID;
        private string hashValue = string.Empty;
        private MOBMPSecurityUpdatePath securityUpdateType;
        private MOBMPPINPWDSecurityItems securityItemsToUpdate;
        private bool rememberDevice;

        public MOBMPSecurityUpdateRequest()
            : base()
        {
        }
        public string SessionID
        {
            get
            {
                return this.sessionID;
            }
            set
            {
                this.sessionID = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        public string MileagePlusNumber
        {
            get
            {
                return this.mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public int CustomerID
        {
            get
            {
                return this.customerID;
            }
            set
            {
                this.customerID = value;
            }
        }

        public string HashValue
        {
            get
            {
                return this.hashValue;
            }
            set
            {
                this.hashValue = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        public MOBMPSecurityUpdatePath SecurityUpdateType
        {
            get
            {
                return this.securityUpdateType;
            }
            set
            {
                this.securityUpdateType = value;
            }
        }

        public MOBMPPINPWDSecurityItems SecurityItemsToUpdate
        {
            get { return securityItemsToUpdate; }
            set { securityItemsToUpdate = value; }
        }
        
        public bool RememberDevice
        {
            get { return rememberDevice; }
            set { rememberDevice = value; }
        }
    }

    [Serializable()]
    public class MOBMPSecurityUpdateResponse : MOBResponse
    {
        public MOBMPSecurityUpdateResponse()
            : base()
        {
        }
        private string mileagePlusNumber = string.Empty;
        public string MileagePlusNumber
        {
            get
            {
                return this.mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        private int customerID;
        public int CustomerID
        {
            get
            {
                return this.customerID;
            }
            set
            {
                this.customerID = value;
            }
        }

        private MOBMPSecurityUpdateRequest request;
        public MOBMPSecurityUpdateRequest Request
        {
            get
            {
                return this.request;
            }
            set
            {
                this.request = value;
            }
        }

        private bool securityUpdate;
        public bool SecurityUpdate
        {
            get { return this.securityUpdate; }
            set { this.securityUpdate = value; }
        }

        private MOBMPPINPWDSecurityUpdateDetails mpSecurityUpdateDetails;
        public MOBMPPINPWDSecurityUpdateDetails MPSecurityUpdateDetails
        {
            get
            {
                return this.mpSecurityUpdateDetails;
            }
            set
            {
                this.mpSecurityUpdateDetails = value;
            }
        }

        private string sessionID = string.Empty;
        public string SessionID
        {
            get
            {
                return this.sessionID;
            }
            set
            {
                this.sessionID = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        private string securityUpdateCompleteMessage;
        public string SecurityUpdateCompleteMessage
        {
            get
            {
                return this.securityUpdateCompleteMessage;
            }
            set
            {
                this.securityUpdateCompleteMessage = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        private string hashValue = string.Empty;

        public string HashValue
        {
            get { return hashValue; }
            set { hashValue = value; }
        }

        private MOBMPTFARememberMeFlags rememberMEFlags;
        public MOBMPTFARememberMeFlags RememberMEFlags
        {
            get
            {
                if (rememberMEFlags == null)
                {
                    rememberMEFlags = new MOBMPTFARememberMeFlags();
                }
                return this.rememberMEFlags;
            }
            set
            {
                this.rememberMEFlags = value;
            }
        }
        private bool showContinueAsGuestButton;
        public bool ShowContinueAsGuestButton
        {
            get
            {
                return showContinueAsGuestButton;
            }
            set { showContinueAsGuestButton = value; }
        }
    }

    #region MPSignInNeedHelp
    [Serializable()]
    public class MOBMPSignInNeedHelpRequest : MOBRequest
    {

        private string sessionID = string.Empty;
        private string mileagePlusNumber = string.Empty;
        private MOBMPSecurityUpdatePath securityUpdateType;
        private MOBMPSignInNeedHelpItems mPSignInNeedHelpItems;

        public MOBMPSignInNeedHelpRequest()
            : base()
        {
        }
        public string SessionID
        {
            get
            {
                return this.sessionID;
            }
            set
            {
                this.sessionID = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        public string MileagePlusNumber
        {
            get
            {
                return this.mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        public MOBMPSecurityUpdatePath SecurityUpdateType
        {
            get
            {
                return this.securityUpdateType;
            }
            set
            {
                this.securityUpdateType = value;
            }
        }

        public MOBMPSignInNeedHelpItems MPSignInNeedHelpItems
        {
            get { return mPSignInNeedHelpItems; }
            set { mPSignInNeedHelpItems = value; }
        }
    }

    [Serializable()]
    public class MOBMPSignInNeedHelpResponse : MOBResponse
    {
        public MOBMPSignInNeedHelpResponse()
            : base()
        {
        }
        private string mileagePlusNumber = string.Empty;
        public string MileagePlusNumber
        {
            get
            {
                return this.mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        private MOBMPSignInNeedHelpRequest request;
        public MOBMPSignInNeedHelpRequest Request
        {
            get
            {
                return this.request;
            }
            set
            {
                this.request = value;
            }
        }

        private MOBMPSignInNeedHelpItemsDetails mpSignInNeedHelpDetails;
        public MOBMPSignInNeedHelpItemsDetails MPSignInNeedHelpDetails
        {
            get
            {
                return this.mpSignInNeedHelpDetails;
            }
            set
            {
                this.mpSignInNeedHelpDetails = value;
            }
        }

        private string sessionID = string.Empty;
        public string SessionID
        {
            get
            {
                return this.sessionID;
            }
            set
            {
                this.sessionID = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        private string needHelpCompleteMessage;
        public string NeedHelpCompleteMessage
        {
            get
            {
                return this.needHelpCompleteMessage;
            }
            set
            {
                this.needHelpCompleteMessage = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        private MOBMPSecurityUpdatePath needHelpSecurityPath;
        public MOBMPSecurityUpdatePath NeedHelpSecurityPath
        {
            get
            {
                return this.needHelpSecurityPath;
            }
            set
            {
                this.needHelpSecurityPath = value;
            }
        }

        private MOBMPTFARememberMeFlags rememberMEFlags;
        public MOBMPTFARememberMeFlags RememberMEFlags
        {
            get
            {
                if (rememberMEFlags == null)
                {
                    rememberMEFlags = new MOBMPTFARememberMeFlags();
                }
                return this.rememberMEFlags;
            }
            set
            {
                this.rememberMEFlags = value;
            }
        }
    }

    [Serializable()]
    public class MOBMPSignInNeedHelpItems
    {
        public MOBMPSignInNeedHelpItems()
            : base()
        {

        }

        private List<Securityquestion> answeredSecurityQuestions;
        public List<Securityquestion> AnsweredSecurityQuestions
        {
            get { return answeredSecurityQuestions; }
            set { answeredSecurityQuestions = value; }
        }

        private MOBName needHelpSignInInfo;
        public MOBName NeedHelpSignInInfo
        {
            get { return needHelpSignInInfo; }
            set { needHelpSignInInfo = value; }
        }

        private string emailAddress;
        public string EmailAddress
        {
            get
            {
                return this.emailAddress;
            }
            set
            {
                this.emailAddress = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        private string mileagePlusNumber;
        public string MileagePlusNumber
        {
            get
            {
                return this.mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }
    
        private string updatedPassword;
        public string UpdatedPassword
        {
            get
            {
                return this.updatedPassword;
            }
            set
            {
                this.updatedPassword = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }
    }

    [Serializable()]
    public class MOBMPSignInNeedHelpItemsDetails
    {
        public MOBMPSignInNeedHelpItemsDetails()
            : base()
        {
        }
        private List<Securityquestion> securityQuestions;
        public List<Securityquestion> SecurityQuestions
        {
            get { return securityQuestions; }
            set { securityQuestions = value; }
        }

        private List<MOBItem> needHelpMessages;
        public List<MOBItem> NeedHelpMessages
        {
            get
            {
                return this.needHelpMessages;
            }
            set
            {
                this.needHelpMessages = value;
            }
        }
    }
    #endregion


    #region MP Enroll
    [Serializable()]
    public class MOBMPEnRollmentRequest : MOBRequest
    {
        public MOBMPEnRollmentRequest()
            : base()
        {

        }

        private string sessionID = string.Empty;
        public string SessionID
        {
            get
            {
                return this.sessionID;
            }
            set
            {
                this.sessionID = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        private bool getSecurityQuestions;
        public bool GetSecurityQuestions
        {
            get { return this.getSecurityQuestions; }
            set { this.getSecurityQuestions = value; }
        }

        private MOBMPMPEnRollmentDetails mpEnrollmentDetails;
        public MOBMPMPEnRollmentDetails MPEnrollmentDetails
        {
            get
            {
                return this.mpEnrollmentDetails;
            }
            set
            {
                this.mpEnrollmentDetails = value;
            }
        }
    }

    [Serializable()]
    public class MOBMPMPEnRollmentDetails
    {
        public MOBMPMPEnRollmentDetails()
            : base()
        {
        }

        private MOBMPEnrollmentPersonalInfo personalInformation;
        public MOBMPEnrollmentPersonalInfo PersonalInformation
        {
            get
            {
                return this.personalInformation;
            }
            set
            {
                this.personalInformation = value;
            }
        }

        private MOBMPEnrollmentContactInfo contactInformation;
        public MOBMPEnrollmentContactInfo ContactInformation
        {
            get
            {
                return this.contactInformation;
            }
            set
            {
                this.contactInformation = value;
            }
        }

        private MOBMPEnrollmentSecurityInfo securityInformation;
        public MOBMPEnrollmentSecurityInfo SecurityInformation
        {
            get
            {
                return this.securityInformation;
            }
            set
            {
                this.securityInformation = value;
            }
        }

        private MOBMPEnrollmentSubscriptions subscriptionPreferences;
        public MOBMPEnrollmentSubscriptions SubscriptionPreferences
        {
            get
            {
                return this.subscriptionPreferences;
            }
            set
            {
                this.subscriptionPreferences = value;
            }
        }
    }

    [Serializable()]
    public class MOBMPMPEnRollmentResponse : MOBResponse
    {
        public MOBMPMPEnRollmentResponse()
            : base()
        {
            needQuestionsCount = 
            Convert.ToInt32(ConfigurationManager.AppSettings["NumberOfSecurityQuestionsNeedatPINPWDUpdate"].ToString());
        }
        private string mileagePlusNumber = string.Empty;
        public string MileagePlusNumber
        {
            get
            {
                return this.mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        private MOBMPEnRollmentRequest request;
        public MOBMPEnRollmentRequest Request
        {
            get
            {
                return this.request;
            }
            set
            {
                this.request = value;
            }
        }

        private string sessionID = string.Empty;
        public string SessionID
        {
            get
            {
                return this.sessionID;
            }
            set
            {
                this.sessionID = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        private string mpEnrollMentCompleteMessage;
        public string MPEnrollMentCompleteMessage
        {
            get
            {
                return this.mpEnrollMentCompleteMessage;
            }
            set
            {
                this.mpEnrollMentCompleteMessage = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }
        
        private MOBMPAccountSummary opAccountSummary;
        public MOBMPAccountSummary OPAccountSummary
        {
            get
            {
                return this.opAccountSummary;
            }
            set
            {
                this.opAccountSummary = value;
            }
        }

        private int needQuestionsCount;
        public int NeedQuestionsCount
        {
            get
            {
                return this.needQuestionsCount;
            }
            set
            {
                this.needQuestionsCount = value;
            }
        }

        private List<Securityquestion> securityQuestions;
        public List<Securityquestion> SecurityQuestions
        {
            get { return securityQuestions; }
            set { securityQuestions = value; }
        }

        private List<MOBItem> mpEnrollmentMessages;
        public List<MOBItem> MPEnrollmentMessages
        {
            get
            {
                return this.mpEnrollmentMessages;
            }
            set
            {
                this.mpEnrollmentMessages = value;
            }
        }

        private MOBMPAccountValidation accountValidation;
        public MOBMPAccountValidation AccountValidation
        {
            get
            {
                return this.accountValidation;
            }
            set
            {
                this.accountValidation = value;
            }
        }

        private MOBMPTFARememberMeFlags rememberMEFlags;
        public MOBMPTFARememberMeFlags RememberMEFlags
        {
            get
            {
                if (rememberMEFlags == null)
                {
                    rememberMEFlags = new MOBMPTFARememberMeFlags();
                }
                return this.rememberMEFlags;
            }
            set
            {
                this.rememberMEFlags = value;
            }
        }
    }

    [Serializable()]
    public class MOBMPEnrollmentPersonalInfo
    {
        private string title = string.Empty;
        private string firstName = string.Empty;
        private string middleName = string.Empty;
        private string lastName = string.Empty;
        private string suffix = string.Empty;
        private string birthDate = string.Empty;
        private string gender = string.Empty;

        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                this.title = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string FirstName
        {
            get
            {
                return this.firstName;
            }
            set
            {
                this.firstName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string MiddleName
        {
            get
            {
                return this.middleName;
            }
            set
            {
                this.middleName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string LastName
        {
            get
            {
                return this.lastName;
            }
            set
            {
                this.lastName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Suffix
        {
            get
            {
                return this.suffix;
            }
            set
            {
                this.suffix = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string BirthDate
        {
            get
            {
                return this.birthDate;
            }
            set
            {
                this.birthDate = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        public string Gender
        {
            get
            {
                return this.gender;
            }
            set
            {
                this.gender = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }
    }

    [Serializable()]
    public class MOBMPEnrollmentContactInfo
    {
        private string streetAddress;

        public string StreetAddress
        {
            get { return streetAddress; }
            set { streetAddress = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string streetAddress2;

        public string StreetAddress2
        {
            get { return streetAddress2; }
            set { streetAddress2 = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string cityRtown;

        public string CityRTown
        {
            get { return cityRtown; }
            set { cityRtown = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private MOBState state;

        public MOBState State
        {
            get { return state; }
            set { state = value; }
        }

        private string zipRpostalCode;

        public string ZipRpostalCode
        {
            get { return zipRpostalCode; }
            set { zipRpostalCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private MOBCountry country;

        public MOBCountry Country
        {
            get { return country; }
            set { country = value; }
        }

        private string phoneNumber;

        public string PhoneNumber
        {
            get { return phoneNumber; }
            set { phoneNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string phoneCountryCode = string.Empty;

        public string PhoneCountryCode
        {
            get
            {
                return this.phoneCountryCode;
            }
            set
            {
                this.phoneCountryCode = string.IsNullOrEmpty(value) ? string.Empty : value.Trim();
            }
        }

        private string emailAddress;

        public string EmailAddress
        {
            get { return emailAddress; }
            set { emailAddress = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }
      
    }

    [Serializable()]
    public class MOBMPEnrollmentSecurityInfo
    {
        private string telephonePIN;
        public string TelephonePIN
        {
            get { return telephonePIN; }
            set { telephonePIN = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string userName;
        public string UserName
        {
            get { return userName; }
            set { userName = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private string passWord;
        public string PassWord
        {
            get { return passWord; }
            set { passWord = string.IsNullOrEmpty(value) ? string.Empty : value.Trim(); }
        }

        private List<Securityquestion> selectedSecurityQuestions;
        public List<Securityquestion> SelectedSecurityQuestions
        {
            get { return selectedSecurityQuestions; }
            set { selectedSecurityQuestions = value; }
        }
    }

    [Serializable]
    public class MOBMPEnrollmentSubscriptions
    {
        private bool unitedNewsnDeals;
        public bool UnitedNewsnDeals
        {
            get { return unitedNewsnDeals; }
            set { unitedNewsnDeals = value; }
        }

        private bool mpPartnerOffers;
        public bool MPPartnerOffers
        {
            get { return mpPartnerOffers; }
            set { mpPartnerOffers = value; }
        }

        private bool mileagePlusProgram;
        public bool MileagePlusProgram
        {
            get { return mileagePlusProgram; }
            set { mileagePlusProgram = value; }
        }

        private bool mileagePlusStmt;
        public bool MileagePlusStmt
        {
            get { return mileagePlusStmt; }
            set { mileagePlusStmt = value; }
        }
               
    }

    #endregion

    #region MOBTFAMPDeviceRequest
    [Serializable()]
    public class MOBTFAMPDeviceRequest : MOBRequest
    {
        public MOBTFAMPDeviceRequest()
            : base()
        {
        }

        private string sessionID = string.Empty;
        public string SessionID
        {
            get
            {
                return this.sessionID;
            }
            set
            {
                this.sessionID = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        private string mileagePlusNumber = string.Empty;
        public string MileagePlusNumber
        {
            get
            {
                return this.mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        private int customerID;
        public int CustomerID
        {
            get
            {
                return this.customerID;
            }
            set
            {
                this.customerID = value;
            }
        }

        private string hashValue = string.Empty;
        public string HashValue
        {
            get
            {
                return this.hashValue;
            }
            set
            {
                this.hashValue = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        private MOBMPSecurityUpdatePath tFAMPDeviceSecurityPath;
        public MOBMPSecurityUpdatePath TFAMPDeviceSecurityPath
        {
            get
            {
                return this.tFAMPDeviceSecurityPath;
            }
            set
            {
                this.tFAMPDeviceSecurityPath = value;
            }
        }

        private List<Securityquestion> answeredSecurityQuestions;
        public List<Securityquestion> AnsweredSecurityQuestions
        {
            get { return answeredSecurityQuestions; }
            set { answeredSecurityQuestions = value; }
        }

        private bool rememberDevice;
        public bool RememberDevice
        {
            get { return rememberDevice; }
            set { rememberDevice = value; }
        }
    }

    [Serializable()]
    public class MOBTFAMPDeviceResponse : MOBResponse
    {
        public MOBTFAMPDeviceResponse()
            : base()
        {
        }

        private string sessionID = string.Empty;
        public string SessionID
        {
            get
            {
                return this.sessionID;
            }
            set
            {
                this.sessionID = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        private string mileagePlusNumber = string.Empty;
        public string MileagePlusNumber
        {
            get
            {
                return this.mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        private int customerID;
        public int CustomerID
        {
            get
            {
                return this.customerID;
            }
            set
            {
                this.customerID = value;
            }
        }

        private string hashValue = string.Empty;
        public string HashValue
        {
            get { return hashValue; }
            set { hashValue = value; }
        }

        private bool securityUpdate;  // This will be true only if Security Questions are anwered incorrectly and this will make client to check the TFAMPDeviceSecurityPath value if to go display thrid question or Accout is locked 
        public bool SecurityUpdate
        {
            get { return this.securityUpdate; }
            set { this.securityUpdate = value; }
        }

        private MOBMPSecurityUpdatePath tFAMPDeviceSecurityPath;
        public MOBMPSecurityUpdatePath TFAMPDeviceSecurityPath
        {
            get
            {
                return this.tFAMPDeviceSecurityPath;
            }
            set
            {
                this.tFAMPDeviceSecurityPath = value;
            }
        }

        private MOBTFAMPDeviceRequest request;
        public MOBTFAMPDeviceRequest Request
        {
            get
            {
                return this.request;
            }
            set
            {
                this.request = value;
            }
        }

        private List<Securityquestion> securityQuestions;
        public List<Securityquestion> SecurityQuestions
        {
            get { return securityQuestions; }
            set { securityQuestions = value; }
        }

        private List<MOBItem> tFAMPDeviceMessages;
        public List<MOBItem> TFAMPDeviceMessages
        {
            get
            {
                return this.tFAMPDeviceMessages;
            }
            set
            {
                this.tFAMPDeviceMessages = value;
            }
        }

        private string tFAMPDeviceCompleteMessage;
        public string TFAMPDeviceCompleteMessage
        {
            get
            {
                return this.tFAMPDeviceCompleteMessage;
            }
            set
            {
                this.tFAMPDeviceCompleteMessage = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        private MOBMPTFARememberMeFlags rememberMEFlags;
        public MOBMPTFARememberMeFlags RememberMEFlags
        {
            get
            {
                if (rememberMEFlags == null)
                {
                    rememberMEFlags = new MOBMPTFARememberMeFlags();
                }
                return this.rememberMEFlags;
            }
            set
            {
                this.rememberMEFlags = value;
            }
        }
        private bool showContinueAsGuestButton;
        public bool ShowContinueAsGuestButton
        {
            get
            {
                return showContinueAsGuestButton;
            }
            set { showContinueAsGuestButton = value; }
        }
    }

    [Serializable()]
    public class MOBMPTFARememberMeFlags
    {
        public MOBMPTFARememberMeFlags()
        {
            showRememberMeDeviceButton = Convert.ToBoolean(ConfigurationManager.AppSettings["ShowRememberMeDeviceButton"].ToString());
            rememberMeDeviceSwitchON = Convert.ToBoolean(ConfigurationManager.AppSettings["RememberMeDeviceSwitchON"].ToString());
            
            string[] strMessages = ConfigurationManager.AppSettings["RememberMEButtonMessages"].ToString().Split('|');

            rememberMEButtonMessages = new List<MOBItem>();
            foreach (string msg in strMessages)
            {
                string id = msg.Split(',')[0];
                string currentValue = msg.Split(',')[1];
                rememberMEButtonMessages.Add(new MOBItem() { Id = id, CurrentValue = currentValue, SaveToPersist = true });
            }
        }

        private List<MOBItem> rememberMEButtonMessages;
        public List<MOBItem> RememberMEButtonMessages
        {
            get
            {
                
                return rememberMEButtonMessages;
            }
            set
            {
                this.rememberMEButtonMessages = value;
            }
        }

        #region Sample values of RememberMEButtonMessages
        //{ 
        //    "MOBMPTFARememberMeFlags": [{
        //        "id": "ButtonTitle",
        //        "currentValue": "Remembe me on this device"
        //    }, {
        //        "id": "ButtonTagLine",
        //        "currentValue": "You won't have to answere security questions again"
        //    }]
        //}
        #endregion

        private bool showRememberMeDeviceButton;
        public bool ShowRememberMeDeviceButton
        {
            get 
            { 
               
                return showRememberMeDeviceButton; 
            }
            set { showRememberMeDeviceButton = value; }
        }

        private bool rememberMeDeviceSwitchON;
        public bool RememberMeDeviceSwitchON
        {
            get
            {
               
                return rememberMeDeviceSwitchON;
            }
            set { rememberMeDeviceSwitchON = value; }
        }
    }

    #endregion

    #region MOBTFAAccountResetEmailLinkRequest
    public class MOBTFAAccountResetEmailRequest : MOBRequest
    {
        public MOBTFAAccountResetEmailRequest()
            : base()
        {
        }

        private string sessionID = string.Empty;
        public string SessionID
        {
            get
            {
                return this.sessionID;
            }
            set
            {
                this.sessionID = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        private string mileagePlusNumber = string.Empty;
        public string MileagePlusNumber
        {
            get
            {
                return this.mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        private int customerID;
        public int CustomerID
        {
            get
            {
                return this.customerID;
            }
            set
            {
                this.customerID = value;
            }
        }

        private string hashValue = string.Empty;
        public string HashValue
        {
            get
            {
                return this.hashValue;
            }
            set
            {
                this.hashValue = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        private MOBMPSecurityUpdatePath tFAMPDeviceSecurityPath;
        public MOBMPSecurityUpdatePath TFAMPDeviceSecurityPath
        {
            get
            {
                return this.tFAMPDeviceSecurityPath;
            }
            set
            {
                this.tFAMPDeviceSecurityPath = value;
            }
        }

        
    }

    public class MOBTFAAccountResetEmailResponse : MOBResponse
    {
        public MOBTFAAccountResetEmailResponse()
            : base()
        {
        }

        private string sessionID = string.Empty;
        public string SessionID
        {
            get
            {
                return this.sessionID;
            }
            set
            {
                this.sessionID = string.IsNullOrEmpty(value) ? string.Empty : value;
            }
        }

        private string mileagePlusNumber = string.Empty;
        public string MileagePlusNumber
        {
            get
            {
                return this.mileagePlusNumber;
            }
            set
            {
                this.mileagePlusNumber = string.IsNullOrEmpty(value) ? string.Empty : value.Trim().ToUpper();
            }
        }

        private int customerID;
        public int CustomerID
        {
            get
            {
                return this.customerID;
            }
            set
            {
                this.customerID = value;
            }
        }

        private string hashValue = string.Empty;
        public string HashValue
        {
            get { return hashValue; }
            set { hashValue = value; }
        }

        private bool securityUpdate;  // This will be true only if Security Questions are anwered incorrectly and this will make client to check the TFAMPDeviceSecurityPath value if to go display thrid question or Accout is locked 
        public bool SecurityUpdate
        {
            get { return this.securityUpdate; }
            set { this.securityUpdate = value; }
        }

        private MOBMPSecurityUpdatePath tFAMPDeviceSecurityPath;
        public MOBMPSecurityUpdatePath TFAMPDeviceSecurityPath
        {
            get
            {
                return this.tFAMPDeviceSecurityPath;
            }
            set
            {
                this.tFAMPDeviceSecurityPath = value;
            }
        }

        private MOBTFAAccountResetEmailResponse request;
        public MOBTFAAccountResetEmailResponse Request
        {
            get
            {
                return this.request;
            }
            set
            {
                this.request = value;
            }
        }

        private List<MOBItem> tFAMPDeviceMessages;
        public List<MOBItem> TFAMPDeviceMessages
        {
            get
            {
                return this.tFAMPDeviceMessages;
            }
            set
            {
                this.tFAMPDeviceMessages = value;
            }
        }

        private MOBMPTFARememberMeFlags rememberMEFlags;
        public MOBMPTFARememberMeFlags RememberMEFlags
        {
            get
            {
                if (rememberMEFlags == null)
                {
                    rememberMEFlags = new MOBMPTFARememberMeFlags();
                }
                return this.rememberMEFlags;
            }
            set
            {
                this.rememberMEFlags = value;
            }
        }
    }
    #endregion  
}
